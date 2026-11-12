# Smoke tests for Alert Management, including response execution authorization.
# Requires the ResQ API running.
param([string]$BaseUrl = "http://localhost:5058")

$ErrorActionPreference = "Stop"
$script:failures = 0

function Check($name, $condition) {
    if ($condition) { Write-Host "  [PASS] $name" -ForegroundColor Green }
    else { Write-Host "  [FAIL] $name" -ForegroundColor Red; $script:failures++ }
}

function Get-StatusCode($scriptBlock) {
    try { & $scriptBlock | Out-Null; return 200 }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            try { return [int]$_.Exception.Response.StatusCode.value__ } catch {}
            try { return [int]$_.Exception.Response.StatusCode } catch {}
        }
        return -1
    }
}

function JsonBody($body) { return ($body | ConvertTo-Json -Depth 20) }

function New-TestUser($prefix) {
    $suffix = [Guid]::NewGuid().ToString("N").Substring(0, 8)
    $username = "$prefix-$suffix"
    $password = "Password123!"
    $body = @{
        firstName = "Alert"
        lastName = "Tester"
        email = "alert.$suffix@resq.com"
        username = $username
        password = $password
        organizationName = "$prefix Organization $suffix"
    }
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -ContentType "application/json" -Body (JsonBody $body)
    return (Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -ContentType "application/json" -Body (JsonBody @{ username = $username; password = $password }))
}

Write-Host "=== SETUP: authenticated admin, building, zone and actuator ==="
$user = New-TestUser "alert"
$h = @{ Authorization = "Bearer $($user.token)" }
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()

$building = Invoke-RestMethod -Uri "$BaseUrl/api/v1/buildings" -Headers $h -Method Post -ContentType "application/json" -Body (JsonBody @{
    buildingCode = "BLD-ALERT-$ts"
    name = "Alert Smoke Building"
    description = "Building for Alert Management smoke tests"
    address = @{ streetAddress = "Av. Arequipa 1200"; district = "Lince"; city = "Lima"; countryCode = "PE" }
})

$buildingWithZone = Invoke-RestMethod -Uri "$BaseUrl/api/v1/buildings/$($building.id)/zones" -Headers $h -Method Post -ContentType "application/json" -Body (JsonBody @{
    zoneCode = "ZON-ALERT-$ts"
    name = "Alert Test Zone"
    description = "Zone for Alert Management smoke tests"
    floorLabel = "Floor 1"
})
$zone = @($buildingWithZone.zones | Where-Object { $_.zoneCode -eq "ZON-ALERT-$ts" })[0]

$device = Invoke-RestMethod -Uri "$BaseUrl/api/v1/devices" -Headers $h -Method Post -ContentType "application/json" -Body (JsonBody @{
    deviceCode = "DEV-ALERT-$ts"
    name = "Alert Actuator"
    description = "Actuator used by Alert Management smoke tests"
    specifications = @{ manufacturer = "ResQ"; model = "ALERT-ACT"; serialNumber = "SN-ALERT-$ts" }
    assignment = @{ buildingId = $building.id; zoneId = $zone.id }
    externalReference = $null
    capabilities = @(
        @{ code = "ALARM_BUZZER"; kind = "actuation"; unit = $null }
    )
})

$device = Invoke-RestMethod -Uri "$BaseUrl/api/v1/devices/$($device.id)/administrative-status" -Headers $h -Method Put -ContentType "application/json" -Body (JsonBody @{ administrativeStatus = "active" })
Check "actuator device is active" ($device.administrativeStatus -eq "active")

Write-Host "\n=== TEST 1: generate alert ==="
$riskDetectionId = "ALERT-SMOKE-$ts"
$alert = Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts" -Headers $h -Method Post -ContentType "application/json" -Body (JsonBody @{
    riskDetectionId = $riskDetectionId
    riskTypeCode = "GAS_LEAK"
    severityCode = "Critical"
    buildingId = $building.id
    zoneId = $zone.id
    detectedAt = [DateTimeOffset]::UtcNow.AddSeconds(-5).ToString("o")
    recipients = @(
        @{ recipientUserId = "$($user.id)"; channel = "PUSH"; destination = "registered-mobile-device" }
    )
    responseActions = @(
        @{ actionCode = "CLOSE_GAS_VALVE"; targetDeviceId = $device.id; targetCapabilityCode = "ALARM_BUZZER"; authorizationMode = "HUMAN_REQUIRED"; critical = $true }
    )
})

Check "POST /alerts -> 201 payload" ($alert.alertId -ne [Guid]::Empty)
Check "alert keeps risk detection snapshot" ($alert.context.riskDetectionId -eq $riskDetectionId)
Check "alert severity is Critical" ($alert.context.severityCode -eq "Critical")
Check "alert has notification delivery" (@($alert.deliveries).Count -eq 1)

$alertId = $alert.alertId

Write-Host "\n=== TEST 2: get alert ==="
$alertById = Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts/$alertId" -Headers $h -Method Get
Check "GET /alerts/{id} -> 200" ($alertById.alertId -eq $alertId)
Check "alert is scoped to current organization" ($alertById.organizationId -eq $user.organizationId)

Write-Host "\n=== TEST 3: list and filter alerts ==="
$alerts = @(Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts?riskTypeCode=GAS_LEAK" -Headers $h -Method Get)
Check "GET /alerts -> 200" ($alerts.Count -ge 1)
Check "riskTypeCode filter finds created alert" (@($alerts | Where-Object { $_.alertId -eq $alertId }).Count -eq 1)

Write-Host "\n=== TEST 4: response executions ==="
$executions = @(Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts/$alertId/response-executions" -Headers $h -Method Get)
Check "GET response executions -> 200" ($executions.Count -eq 1)
Check "human-required execution starts pending authorization" ($executions[0].status -eq "PENDING_AUTHORIZATION")
$executionId = $executions[0].responseExecutionId
if (-not $executionId) { $executionId = $executions[0].id }

Write-Host "\n=== TEST 5: authorize response execution ==="
$authorized = Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts/$alertId/response-executions/$executionId/authorization" -Headers $h -Method Put -ContentType "application/json" -Body (JsonBody @{ decision = "APPROVED" })
Check "authorization endpoint -> 200" ($authorized -ne $null)
Check "execution becomes authorized" ($authorized.status -eq "AUTHORIZED")

Write-Host "\n=== TEST 6: validation errors ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts?from=2026-11-12T10:00:00Z&to=2026-11-12T09:00:00Z" -Headers $h -Method Get
}
Check "from later than to -> 400 (got $code)" ($code -eq 400)

$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts/$([Guid]::NewGuid())" -Headers $h -Method Get
}
Check "unknown alert -> 404 (got $code)" ($code -eq 404)

Write-Host "\n=== TEST 7: unauthorized ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts" -Method Get
}
Check "GET /alerts without JWT -> 401 (got $code)" ($code -eq 401)

Write-Host ""
if ($script:failures -eq 0) { Write-Host "ALL ALERT TESTS PASSED" -ForegroundColor Green }
else { Write-Host "$($script:failures) ALERT TEST(S) FAILED" -ForegroundColor Red; exit 1 }
