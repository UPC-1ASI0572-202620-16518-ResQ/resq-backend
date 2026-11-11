# Smoke test of the Connectivity Management bounded context.
# Requires the API running. -TimeoutSeconds must match ConnectivitySettings:HeartbeatTimeoutSeconds of the running API
# (60 by default; to make the test faster start the API with ConnectivitySettings__HeartbeatTimeoutSeconds=10).
param(
    [string]$BaseUrl = "http://localhost:5058",
    [int]$TimeoutSeconds = 60,
    [int]$ScanIntervalSeconds = 15
)

$ErrorActionPreference = "Stop"
$script:failures = 0

function Check($name, $condition) {
    if ($condition) { Write-Host "  [PASS] $name" -ForegroundColor Green }
    else { Write-Host "  [FAIL] $name" -ForegroundColor Red; $script:failures++ }
}

function Get-StatusCode($scriptBlock) {
    try { & $scriptBlock | Out-Null; return 200 }
    catch { return [int]$_.Exception.Response.StatusCode }
}

# 1. Login or register test user
$username = "testconnectivity"
$password = "Password123!"
try {
    $signInBody = @{ username = $username; password = $password } | ConvertTo-Json
    $token = (Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -Body $signInBody -ContentType "application/json").token
} catch {
    $signUpBody = @{
        firstName = "Connectivity"; lastName = "Tester"; email = "testconnectivity@resq.com"; phone = "987654322"
        username = $username; password = $password; role = 0
    } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -Body $signUpBody -ContentType "application/json"
    $token = (Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -Body $signInBody -ContentType "application/json").token
}
$h = @{ "Authorization" = "Bearer $token" }
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()

Write-Host "=== SETUP: building, zone and device ==="
$bld = Invoke-RestMethod -Uri "$BaseUrl/api/v1/buildings" -Method Post -Headers $h -ContentType "application/json" -Body (@{
    buildingCode = "BLD-CON-$ts"; name = "Connectivity Lab"; description = "Heartbeat tests"
    address = @{ streetAddress = "Av. Primavera 2390"; district = "Santiago de Surco"; city = "Lima"; countryCode = "PE" }
} | ConvertTo-Json -Depth 5)
$bldWithZone = Invoke-RestMethod -Uri "$BaseUrl/api/v1/buildings/$($bld.id)/zones" -Method Post -Headers $h -ContentType "application/json" -Body (@{
    zoneCode = "ZON-NET-01"; name = "Network room"; description = "Gateway"; floorLabel = "Floor 1"
} | ConvertTo-Json)
$zoneId = $bldWithZone.zones[0].id
$dev = Invoke-RestMethod -Uri "$BaseUrl/api/v1/devices" -Method Post -Headers $h -ContentType "application/json" -Body (@{
    deviceCode = "DEV-CON-$ts"; name = "MQ-2 node"; description = "ESP32 + MQ-2"
    specifications = @{ manufacturer = "Espressif"; model = "ESP32"; serialNumber = "SN-CON-$ts" }
    assignment = @{ buildingId = $bld.id; zoneId = $zoneId }
    externalReference = $null
    capabilities = @(@{ code = "GAS_PPM"; kind = "measurement"; unit = "PPM" })
} | ConvertTo-Json -Depth 5)
$sensorId = $dev.id
Write-Host "Building=$($bld.id) Zone=$zoneId Device(sensor)=$sensorId"

Write-Host "`n=== TEST 1: status before any heartbeat (404 expected) ==="
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/sensors/$sensorId/status" -Headers $h }
Check "GET status of a sensor without heartbeats -> 404 (got $code)" ($code -eq 404)

Write-Host "`n=== TEST 2: heartbeat of an unknown device (404 expected) ==="
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/heartbeats" -Method Post -Headers $h -ContentType "application/json" -Body (@{ sensorId = [guid]::NewGuid(); signalStrength = -50 } | ConvertTo-Json) }
Check "POST heartbeat unknown sensor -> 404 (got $code)" ($code -eq 404)

Write-Host "`n=== TEST 3: heartbeat with signal out of range (400 expected) ==="
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/heartbeats" -Method Post -Headers $h -ContentType "application/json" -Body (@{ sensorId = $sensorId; signalStrength = 20 } | ConvertTo-Json) }
Check "POST heartbeat signalStrength=20 -> 400 (got $code)" ($code -eq 400)

Write-Host "`n=== TEST 4: first heartbeat opens the connection as ONLINE ==="
$hb = Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/heartbeats" -Method Post -Headers $h -ContentType "application/json" -Body (@{ sensorId = $sensorId; signalStrength = -65 } | ConvertTo-Json)
$hb | ConvertTo-Json -Compress | Write-Host
Check "status ONLINE" ($hb.status -eq "ONLINE")
Check "signalStrength -65" ($hb.signalStrength -eq -65)
Check "sensorId echoed" ($hb.sensorId -eq $sensorId)

Write-Host "`n=== TEST 5: GET status ==="
$st = Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/sensors/$sensorId/status" -Headers $h
$st | ConvertTo-Json -Compress | Write-Host
Check "status ONLINE with lastHeartbeatTime" ($st.status -eq "ONLINE" -and $st.lastHeartbeatTime)

Write-Host "`n=== TEST 6: second heartbeat updates the signal ==="
$hb2 = Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/heartbeats" -Method Post -Headers $h -ContentType "application/json" -Body (@{ sensorId = $sensorId; signalStrength = -40 } | ConvertTo-Json)
Check "signalStrength -40, still ONLINE" ($hb2.signalStrength -eq -40 -and $hb2.status -eq "ONLINE")

Write-Host "`n=== TEST 7: offline list does not contain an ONLINE sensor ==="
$offline = @(Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/sensors/offline" -Headers $h)
Check "sensor not in offline list" (-not ($offline | Where-Object { $_.sensorId -eq $sensorId }))

$wait = $TimeoutSeconds + $ScanIntervalSeconds + 3
Write-Host "`n=== TEST 8: no heartbeats for $wait s -> TIMEOUT ==="
Start-Sleep -Seconds $wait
$st = Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/sensors/$sensorId/status" -Headers $h
$st | ConvertTo-Json -Compress | Write-Host
Check "status TIMEOUT" ($st.status -eq "TIMEOUT")
$offline = @(Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/sensors/offline" -Headers $h)
Check "sensor listed in /sensors/offline" (($offline | Where-Object { $_.sensorId -eq $sensorId -and $_.status -eq "TIMEOUT" }) -ne $null)

Write-Host "`n=== TEST 9: disconnection alert generated in Alert Management ==="
$alerts = @(Invoke-RestMethod -Uri "$BaseUrl/api/v1/alerts?riskTypeCode=SENSOR_DISCONNECTED&buildingId=$($bld.id)" -Headers $h)
$alerts | Select-Object alertId, @{n="riskDetectionId";e={$_.context.riskDetectionId}}, @{n="severity";e={$_.context.severityCode}} | Format-Table | Out-String | Write-Host
Check "one SENSOR_DISCONNECTED alert for the building" ($alerts.Count -ge 1 -and $alerts[0].context.zoneId -eq $zoneId)

Write-Host "`n=== TEST 10: a new heartbeat brings the sensor back ONLINE ==="
$hb3 = Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/heartbeats" -Method Post -Headers $h -ContentType "application/json" -Body (@{ sensorId = $sensorId } | ConvertTo-Json)
Check "TIMEOUT -> ONLINE (got $($hb3.status)), signal null" ($hb3.status -eq "ONLINE" -and $null -eq $hb3.signalStrength)
$offline = @(Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/sensors/offline" -Headers $h)
Check "sensor removed from offline list" (-not ($offline | Where-Object { $_.sensorId -eq $sensorId }))

Write-Host "`n=== TEST 11: no token -> 401 ==="
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/connectivity/sensors/offline" }
Check "GET offline without token -> 401 (got $code)" ($code -eq 401)

Write-Host ""
if ($script:failures -eq 0) { Write-Host "ALL CONNECTIVITY TESTS PASSED" -ForegroundColor Green }
else { Write-Host "$($script:failures) CONNECTIVITY TEST(S) FAILED" -ForegroundColor Red; exit 1 }
