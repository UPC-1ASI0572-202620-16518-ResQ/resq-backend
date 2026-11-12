# Smoke test of the Monitoring bounded context and its chain with Device Management,
# Connectivity Management, Risk Detection and Alert Management.
# Requires the API running.
param(
    [string]$BaseUrl = "http://localhost:5058"
)

$ErrorActionPreference = "Stop"
$script:failures = 0

function Check($name, $condition) {
    if ($condition) { Write-Host "  [PASS] $name" -ForegroundColor Green }
    else { Write-Host "  [FAIL] $name" -ForegroundColor Red; $script:failures++ }
}

# Windows PowerShell 5.1 returns a JSON array as a single object; this enumerates it.
function Get-List($path) {
    $response = Invoke-RestMethod -Uri "$BaseUrl$path" -Headers $h
    return ,@($response | ForEach-Object { $_ })
}

function Get-StatusCode($scriptBlock) {
    try { & $scriptBlock | Out-Null; return 200 }
    catch { return [int]$_.Exception.Response.StatusCode }
}

function Post($path, $body) {
    Invoke-RestMethod -Uri "$BaseUrl$path" -Method Post -Headers $h -ContentType "application/json" -Body ($body | ConvertTo-Json -Depth 10)
}

Write-Host "=== SETUP: authentication, building, zone and active device ==="
$username = "testmonitoring"
$password = "Password123!"

try {
    $signInBody = @{ username = $username; password = $password } | ConvertTo-Json
    $token = (Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -Body $signInBody -ContentType "application/json").token
} catch {
    $signUpBody = @{
        firstName = "Monitoring"; lastName = "Tester"; email = "testmonitoring@resq.com"; phone = "987654329"
        username = $username; password = $password; role = 0
    } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -Body $signUpBody -ContentType "application/json"
    $token = (Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -Body (@{ username = $username; password = $password } | ConvertTo-Json) -ContentType "application/json").token
}
$h = @{ "Authorization" = "Bearer $token" }
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$variableType = "GAS_PPM_$ts"
$oldVariableType = "OLD_GAS_PPM_$ts"

$bld = Post "/api/v1/buildings" @{
    buildingCode = "BLD-MON-$ts"
    name = "Monitoring Lab"
    description = "Cloud Monitoring acceptance tests"
    address = @{ streetAddress = "Av. Arequipa 1200"; district = "Lince"; city = "Lima"; countryCode = "PE" }
}

$bldWithZone = Post "/api/v1/buildings/$($bld.id)/zones" @{
    zoneCode = "ZON-MON-01"
    name = "Monitoring Lab Zone"
    description = "Zone used by Monitoring tests"
    floorLabel = "Floor 1"
}
$zoneId = $bldWithZone.zones[0].id

$dev = Post "/api/v1/devices" @{
    deviceCode = "DEV-MON-$ts"
    name = "Monitoring Gas Sensor"
    description = "Sensor for Monitoring acceptance tests"
    specifications = @{ manufacturer = "ResQ"; model = "MQ-2"; serialNumber = "SN-MON-$ts" }
    assignment = @{ buildingId = $bld.id; zoneId = $zoneId }
    externalReference = $null
    capabilities = @(
        @{ code = $variableType; kind = "measurement"; unit = "PPM" },
        @{ code = $oldVariableType; kind = "measurement"; unit = "PPM" }
    )
}
$deviceId = $dev.id

# Devices are registered as INACTIVE by design. Activate before sending measurements.
$activeDevice = Invoke-RestMethod -Uri "$BaseUrl/api/v1/devices/$deviceId/administrative-status" -Method Put -Headers $h -ContentType "application/json" -Body (@{ administrativeStatus = "active" } | ConvertTo-Json)
Check "device activated" ($activeDevice.administrativeStatus -eq "active")

# Create a connectivity session so Monitoring can classify the device as AVAILABLE.
$heartbeat = Post "/api/v1/connectivity/heartbeats" @{ sensorId = $deviceId; signalStrength = -55 }
Check "device connectivity ONLINE" ($heartbeat.status -eq "ONLINE")

Write-Host "Building=$($bld.id) Zone=$zoneId Device=$deviceId VariableType=$variableType"

Write-Host "`n=== TEST 1: configure Risk Detection rule used by Monitoring ==="
$rule = Post "/api/v1/risk-detection/rules" @{
    riskTypeCode = "GAS_LEAK"
    severityCode = "Critical"
    variableType = $variableType
    operator = "GREATER_THAN"
    threshold = 600
}
Check "GAS_LEAK Critical rule ACTIVE" (
    $rule.status -eq "ACTIVE" -and
    $rule.riskTypeCode -eq "GAS_LEAK" -and
    $rule.severityCode -eq "Critical" -and
    $rule.condition.operator -eq "GREATER_THAN" -and
    $rule.condition.threshold -eq 600)

Write-Host "`n=== TEST 2: normal measurement -> persisted, no risk ==="
$normalMeasuredAt = [DateTimeOffset]::UtcNow.AddSeconds(-20)
$normal = Post "/api/v1/monitoring/measurements" @{
    deviceId = $deviceId
    variableType = $variableType
    value = 120
    unit = "PPM"
    measuredAt = $normalMeasuredAt.ToString("o")
}
$normalMeasurement = $normal
Check "normal measurement returns 201 payload" ($normalMeasurement.measurementId -and $normalMeasurement.deviceId -eq $deviceId)
Check "normal measurement keeps building and zone snapshot" ($normalMeasurement.buildingId -eq $bld.id -and $normalMeasurement.zoneId -eq $zoneId)
Check "normal measurement has no riskDetectionIds" (@($normal.riskDetectionIds).Count -eq 0)

Write-Host "`n=== TEST 3: high measurement -> Monitoring -> Risk Detection -> Alert Management ==="

$highMeasuredAt = [DateTimeOffset]::UtcNow.AddSeconds(-10)

$high = Post "/api/v1/monitoring/measurements" @{
    deviceId = $deviceId
    variableType = $variableType
    value = 750
    unit = "PPM"
    measuredAt = $highMeasuredAt.ToString("o")
}

$highMeasurement = $high
$riskDetectionIds = @($high.riskDetectionIds)

Check "high measurement returns a riskDetectionId" ($riskDetectionIds.Count -ge 1)
Check "measurement id is reused for risk evaluation" ($highMeasurement.measurementId)

$detId = $riskDetectionIds[0]

$detections = Get-List "/api/v1/risk-detections?buildingId=$($bld.id)&zoneId=$zoneId&riskTypeCode=GAS_LEAK"

Check "Risk Detection contains the detection created from Monitoring" (
    @($detections | Where-Object {
        $_.riskDetectionId -eq $detId
    }).Count -ge 1
)

$alerts = Get-List "/api/v1/alerts?buildingId=$($bld.id)&riskTypeCode=GAS_LEAK"

Check "Alert Management contains an alert for the detection" (
    @($alerts | Where-Object {
        $_.context.riskDetectionId -eq $detId
    }).Count -ge 1
)

Write-Host "`n=== TEST 4: Monitoring states ==="
$deviceStatus = Invoke-RestMethod -Uri "$BaseUrl/api/v1/monitoring/devices/$deviceId/status" -Headers $h
Check "device status AVAILABLE from Connectivity ONLINE" ($deviceStatus.deviceId -eq $deviceId -and $deviceStatus.availability -eq "AVAILABLE")
Check "device status keeps lastMeasurementAt" ($deviceStatus.lastMeasurementAt)

$zoneStatus = Invoke-RestMethod -Uri "$BaseUrl/api/v1/monitoring/zones/$zoneId/status" -Headers $h
Check "zone status belongs to building" ($zoneStatus.zoneId -eq $zoneId -and $zoneStatus.buildingId -eq $bld.id)
Check "zone status marks active risk" ($zoneStatus.activeRiskId -eq $detId)
Check "zone status condition is RISK_ACTIVE" ($zoneStatus.conditionCode -eq "RISK_ACTIVE")

$buildingStatus = Invoke-RestMethod -Uri "$BaseUrl/api/v1/monitoring/buildings/$($bld.id)/status" -Headers $h
Check "building status returns the monitored zone" (@($buildingStatus.zones).Count -ge 1 -and $buildingStatus.zones[0].zoneId -eq $zoneId)
Check "building status counts the available device" ($buildingStatus.availableDevices -ge 1)

Write-Host "`n=== TEST 5: current measurements by device and zone ==="

$currentDevice = Get-List "/api/v1/monitoring/devices/$deviceId/measurements/current"

Check "current device measurements contain GAS_PPM value 750" (
    @($currentDevice | Where-Object {
        $_.variableType -eq $variableType -and
        [decimal]$_.measurementValue.value -eq 750
    }).Count -eq 1
)

$currentZone = Get-List "/api/v1/monitoring/zones/$zoneId/measurements/current"

Check "current zone measurements contain GAS_PPM value 750" (
    @($currentZone | Where-Object {
        $_.variableType -eq $variableType -and
        [decimal]$_.measurementValue.value -eq 750
    }).Count -eq 1
)

Write-Host "`n=== TEST 6: stale measurement is not returned as current ==="
$old = Post "/api/v1/monitoring/measurements" @{
    deviceId = $deviceId
    variableType = $oldVariableType
    value = 50
    unit = "PPM"
    measuredAt = [DateTimeOffset]::UtcNow.AddHours(-1).ToString("o")
}
$currentAfterOld = Get-List "/api/v1/monitoring/devices/$deviceId/measurements/current"
Check "1-hour-old measurement is excluded from current measurements" (-not ($currentAfterOld | Where-Object { $_.variableType -eq $oldVariableType }))

Write-Host "`n=== TEST 7: validation errors ==="
$code = Get-StatusCode { Post "/api/v1/monitoring/measurements" @{
    deviceId = [guid]::NewGuid()
    variableType = $variableType
    value = 100
    unit = "PPM"
    measuredAt = [DateTimeOffset]::UtcNow.AddSeconds(-1).ToString("o")
} }
Check "unknown device -> 404 (got $code)" ($code -eq 404)

$code = Get-StatusCode { Post "/api/v1/monitoring/measurements" @{
    deviceId = $deviceId
    variableType = $variableType
    value = 100
    unit = "PPM"
    measuredAt = [DateTimeOffset]::UtcNow.AddMinutes(10).ToString("o")
} }
Check "future measuredAt -> 400 (got $code)" ($code -eq 400)

$code = Get-StatusCode { Post "/api/v1/monitoring/measurements" @{
    deviceId = $deviceId
    variableType = $variableType
    unit = "PPM"
    measuredAt = [DateTimeOffset]::UtcNow.AddSeconds(-1).ToString("o")
} }
Check "missing value -> 400 (got $code)" ($code -eq 400)

Write-Host "`n=== TEST 8: unauthorized request ==="
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/monitoring/devices/$deviceId/status" }
Check "GET device status without token -> 401 (got $code)" ($code -eq 401)

Write-Host ""
if ($script:failures -eq 0) {
    Write-Host "ALL MONITORING TESTS PASSED" -ForegroundColor Green
}
else {
    Write-Host "$($script:failures) MONITORING TEST(S) FAILED" -ForegroundColor Red
    exit 1
}
