# Smoke test of the Risk Detection bounded context and its chain with Device Management and Alert Management.
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

# Windows PowerShell 5.1 returns a JSON array as a single object; this enumerates it
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

# 1. Login or register test user
$username = "testriskdetection"
$password = "Password123!"
try {
    $signInBody = @{ username = $username; password = $password } | ConvertTo-Json
    $token = (Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -Body $signInBody -ContentType "application/json").token
} catch {
    $signUpBody = @{
        firstName = "Risk"; lastName = "Tester"; email = "testriskdetection@resq.com"; phone = "987654323"
        username = $username; password = $password; role = 0
    } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -Body $signUpBody -ContentType "application/json"
    $token = (Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -Body $signInBody -ContentType "application/json").token
}
$h = @{ "Authorization" = "Bearer $token" }
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
# A variable type per run keeps the rules of previous runs out of this evaluation
$variableType = "GAS_PPM_$ts"

Write-Host "=== SETUP: building, zone and device ==="
$bld = Post "/api/v1/buildings" @{
    buildingCode = "BLD-RSK-$ts"; name = "Risk Lab"; description = "Risk detection tests"
    address = @{ streetAddress = "Av. Arequipa 1200"; district = "Lince"; city = "Lima"; countryCode = "PE" }
}
$zoneId = (Post "/api/v1/buildings/$($bld.id)/zones" @{ zoneCode = "ZON-KIT-01"; name = "Kitchen"; description = "Gas stove"; floorLabel = "Floor 1" }).zones[0].id
$dev = Post "/api/v1/devices" @{
    deviceCode = "DEV-RSK-$ts"; name = "MQ-2 kitchen node"; description = "ESP32 + MQ-2"
    specifications = @{ manufacturer = "Espressif"; model = "ESP32"; serialNumber = "SN-RSK-$ts" }
    assignment = @{ buildingId = $bld.id; zoneId = $zoneId }
    externalReference = $null
    capabilities = @(@{ code = $variableType; kind = "measurement"; unit = "PPM" })
}
Write-Host "Building=$($bld.id) Zone=$zoneId Device=$($dev.id) VariableType=$variableType"

Write-Host "`n=== TEST 1: invalid rules (400 expected) ==="
$code = Get-StatusCode { Post "/api/v1/risk-detection/rules" @{ riskTypeCode = "GAS_LEAK"; severityCode = "Critical"; variableType = $variableType; operator = "BIGGER"; threshold = 1 } }
Check "invalid operator -> 400 (got $code)" ($code -eq 400)
$code = Get-StatusCode { Post "/api/v1/risk-detection/rules" @{ riskTypeCode = "GAS_LEAK"; severityCode = "Extreme"; variableType = $variableType; operator = "GREATER_THAN"; threshold = 1 } }
Check "invalid severity -> 400 (got $code)" ($code -eq 400)

Write-Host "`n=== TEST 2: configure rules (Warning > 300, Critical >= 600) ==="
$warn = Post "/api/v1/risk-detection/rules" @{ riskTypeCode = "gas_leak"; severityCode = "warning"; variableType = $variableType; operator = "GREATER_THAN"; threshold = 300 }
$warn | ConvertTo-Json -Compress -Depth 5 | Write-Host
Check "rule Warning ACTIVE, codes normalized" ($warn.status -eq "ACTIVE" -and $warn.riskTypeCode -eq "GAS_LEAK" -and $warn.severityCode -eq "Warning" -and $warn.condition.operator -eq "GREATER_THAN" -and $warn.condition.threshold -eq 300)
$crit = Post "/api/v1/risk-detection/rules" @{ riskTypeCode = "GAS_LEAK"; severityCode = "Critical"; variableType = $variableType; operator = "GREATER_THAN_OR_EQUAL"; threshold = 600 }
Check "rule Critical ACTIVE" ($crit.status -eq "ACTIVE" -and $crit.condition.operator -eq "GREATER_THAN_OR_EQUAL")
$rules = (Get-List "/api/v1/risk-detection/rules?variableType=$variableType")
Check "GET rules?variableType -> 2 rules (got $($rules.Count))" ($rules.Count -eq 2)
$one = Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detection/rules/$($warn.ruleId)" -Headers $h
Check "GET rule by id" ($one.ruleId -eq $warn.ruleId)

$now = [DateTimeOffset]::UtcNow

Write-Host "`n=== TEST 3: measurement below every threshold -> no detection ==="
$ev = Post "/api/v1/risk-detections/evaluations" @{ deviceId = $dev.id; variableType = $variableType; value = 120; measuredAt = $now.AddSeconds(-30).ToString("o") }
Check "riskDetected=false, evaluatedRules=2" (-not $ev.riskDetected -and $ev.evaluatedRules -eq 2 -and @($ev.generatedAlertIds).Count -eq 0)

Write-Host "`n=== TEST 4: 450 ppm -> new Warning detection + alert ==="
$m1 = [guid]::NewGuid().ToString()
$ev = Post "/api/v1/risk-detections/evaluations" @{ deviceId = $dev.id; variableType = $variableType; value = 450; measuredAt = $now.AddSeconds(-20).ToString("o"); measurementId = $m1 }
$det = $ev.riskDetections[0]
$ev | ConvertTo-Json -Compress -Depth 6 | Write-Host
Check "riskDetected, Warning, rule = Warning rule" ($ev.riskDetected -and $det.currentSeverityCode -eq "Warning" -and $det.ruleId -eq $warn.ruleId)
Check "location RESOLVED to building/zone of the device" ($det.location.resolutionStatus -eq "RESOLVED" -and $det.location.buildingId -eq $bld.id -and $det.location.zoneId -eq $zoneId)
Check "evidence keeps measurementId/deviceId/value" ($det.evidence[0].measurementId -eq $m1 -and $det.evidence[0].deviceId -eq $dev.id -and $det.evidence[0].value -eq 450)
Check "one alert generated" (@($ev.generatedAlertIds).Count -eq 1)
$detId = $det.riskDetectionId

Write-Host "`n=== TEST 5: GET /risk-detections/{id} ==="
$got = Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detections/$detId" -Headers $h
Check "detection found with 1 evidence" ($got.riskDetectionId -eq $detId -and @($got.evidence).Count -eq 1 -and @($got.severityChanges).Count -eq 0)

Write-Host "`n=== TEST 6: 750 ppm -> same detection escalates to Critical + new alert ==="
$ev = Post "/api/v1/risk-detections/evaluations" @{ deviceId = $dev.id; variableType = $variableType; value = 750; measuredAt = $now.AddSeconds(-10).ToString("o") }
$det = $ev.riskDetections[0]
Check "same detection id" ($det.riskDetectionId -eq $detId)
Check "severity Critical by the Critical rule" ($det.currentSeverityCode -eq "Critical" -and $det.ruleId -eq $crit.ruleId)
Check "severityChange Warning -> Critical" (@($det.severityChanges).Count -eq 1 -and $det.severityChanges[0].previousSeverityCode -eq "Warning" -and $det.severityChanges[0].newSeverityCode -eq "Critical")
Check "escalation generated one alert" (@($ev.generatedAlertIds).Count -eq 1)

Write-Host "`n=== TEST 7: GET /risk-detections/{id}/evidence ==="
$evd = Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detections/$detId/evidence" -Headers $h
$evd.evidence | Format-Table measurementId, value, measuredAt | Out-String | Write-Host
Check "2 evidence entries (450, 750)" (@($evd.evidence).Count -eq 2 -and $evd.evidence[0].value -eq 450 -and $evd.evidence[1].value -eq 750)

Write-Host "`n=== TEST 8: alerts created in Alert Management for this detection ==="
$alerts = (Get-List "/api/v1/alerts?buildingId=$($bld.id)&riskTypeCode=GAS_LEAK")
$alerts | ForEach-Object { "{0}  {1}  {2}" -f $_.alertId, $_.context.riskDetectionId, $_.context.severityCode } | Write-Host
Check "2 alerts referencing the riskDetectionId (Warning + Critical)" (
    @($alerts | Where-Object { $_.context.riskDetectionId -eq $detId }).Count -eq 2 -and
    ($alerts | Where-Object { $_.context.severityCode -eq "Critical" }) -and
    ($alerts | Where-Object { $_.context.severityCode -eq "Warning" }))

Write-Host "`n=== TEST 9: PATCH rule status ==="
$patched = Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detection/rules/$($crit.ruleId)/status" -Method Patch -Headers $h -ContentType "application/json" -Body (@{ active = $false } | ConvertTo-Json)
Check "Critical rule INACTIVE" ($patched.status -eq "INACTIVE")
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detection/rules/$($crit.ruleId)/status" -Method Patch -Headers $h -ContentType "application/json" -Body "{}" }
Check "PATCH without 'active' -> 400 (got $code)" ($code -eq 400)
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detection/rules/$([guid]::NewGuid())/status" -Method Patch -Headers $h -ContentType "application/json" -Body (@{ active = $true } | ConvertTo-Json) }
Check "PATCH unknown rule -> 404 (got $code)" ($code -eq 404)
$active = (Get-List "/api/v1/risk-detection/rules?variableType=$variableType&status=ACTIVE")
Check "only 1 ACTIVE rule left" ($active.Count -eq 1 -and $active[0].ruleId -eq $warn.ruleId)

Write-Host "`n=== TEST 10: 800 ppm with Critical rule inactive -> only Warning applies ==="
$ev = Post "/api/v1/risk-detections/evaluations" @{ deviceId = $dev.id; variableType = $variableType; value = 800; measuredAt = $now.ToString("o") }
$det = $ev.riskDetections[0]
Check "evaluatedRules=1, same detection, severity back to Warning, traced" ($ev.evaluatedRules -eq 1 -and $det.riskDetectionId -eq $detId -and $det.currentSeverityCode -eq "Warning" -and @($det.severityChanges).Count -eq 2)
Check "de-escalation does not raise an alert" (@($ev.generatedAlertIds).Count -eq 0)

Write-Host "`n=== TEST 11: unknown device -> UNRESOLVED location ==="
$ev = Post "/api/v1/risk-detections/evaluations" @{ deviceId = [guid]::NewGuid(); variableType = $variableType; value = 500; measuredAt = $now.ToString("o") }
Check "new detection with UNRESOLVED location" ($ev.riskDetections[0].location.resolutionStatus -eq "UNRESOLVED" -and $null -eq $ev.riskDetections[0].location.buildingId -and $ev.riskDetections[0].riskDetectionId -ne $detId)

Write-Host "`n=== TEST 12: list detections ==="
$list = (Get-List "/api/v1/risk-detections?buildingId=$($bld.id)")
Check "GET risk-detections?buildingId -> 1 (got $($list.Count))" ($list.Count -eq 1 -and $list[0].riskDetectionId -eq $detId)
$all = (Get-List "/api/v1/risk-detections?riskTypeCode=GAS_LEAK")
Check "GET risk-detections?riskTypeCode includes both detections" ($all.Count -ge 2)

Write-Host "`n=== TEST 13: error cases ==="
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detections/$([guid]::NewGuid())" -Headers $h }
Check "GET unknown detection -> 404 (got $code)" ($code -eq 404)
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detections/$([guid]::NewGuid())/evidence" -Headers $h }
Check "GET evidence of unknown detection -> 404 (got $code)" ($code -eq 404)
$code = Get-StatusCode { Post "/api/v1/risk-detections/evaluations" @{ deviceId = $dev.id; variableType = $variableType; measuredAt = $now.ToString("o") } }
Check "evaluation without value -> 400 (got $code)" ($code -eq 400)
$code = Get-StatusCode { Post "/api/v1/risk-detections/evaluations" @{ deviceId = $dev.id; variableType = $variableType; value = 900; measuredAt = $now.AddDays(1).ToString("o") } }
Check "evaluation in the future -> 400 (got $code)" ($code -eq 400)
$code = Get-StatusCode { Invoke-RestMethod -Uri "$BaseUrl/api/v1/risk-detection/rules" }
Check "no token -> 401 (got $code)" ($code -eq 401)

Write-Host ""
if ($script:failures -eq 0) { Write-Host "ALL RISK DETECTION TESTS PASSED" -ForegroundColor Green }
else { Write-Host "$($script:failures) RISK DETECTION TEST(S) FAILED" -ForegroundColor Red; exit 1 }
