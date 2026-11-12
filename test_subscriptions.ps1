# Smoke tests for Subscriptions bounded context.
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

Write-Host "=== SETUP: unique subscription test organization ==="
$suffix = [Guid]::NewGuid().ToString("N").Substring(0, 8)
$username = "subscription-$suffix"
$password = "Password123!"
$signUpBody = @{
    firstName = "Subscription"
    lastName = "Tester"
    email = "subscription.$suffix@resq.com"
    username = $username
    password = $password
    organizationName = "Subscription Organization $suffix"
}
$null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -ContentType "application/json" -Body (JsonBody $signUpBody)
$user = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -ContentType "application/json" -Body (JsonBody @{ username = $username; password = $password })
$h = @{ Authorization = "Bearer $($user.token)" }

Write-Host "\n=== TEST 1: invalid subscription dates ==="
$invalidStart = [DateTime]::UtcNow.AddDays(2).ToString("o")
$invalidEnd = [DateTime]::UtcNow.AddDays(1).ToString("o")
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions" -Headers $h -Method Post -ContentType "application/json" -Body (JsonBody @{ startDate = $invalidStart; endDate = $invalidEnd })
}
Check "endDate <= startDate -> 400 (got $code)" ($code -eq 400)

Write-Host "\n=== TEST 2: create subscription ==="
$start = [DateTime]::UtcNow.AddDays(-1)
$end = [DateTime]::UtcNow.AddDays(30)
$subscription = Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions" -Headers $h -Method Post -ContentType "application/json" -Body (JsonBody @{ startDate = $start.ToString("o"); endDate = $end.ToString("o") })
Check "POST /subscriptions -> 201" ($subscription.id -ne [Guid]::Empty)
Check "new subscription is Active" ($subscription.status -eq "Active")
Check "subscription belongs to current organization" ($subscription.organizationId -eq $user.organizationId)
$subscriptionId = $subscription.id

Write-Host "\n=== TEST 3: get subscription ==="
$current = Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions" -Headers $h -Method Get
Check "GET /subscriptions -> 200" ($current.id -eq $subscriptionId)

$byId = Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions/$subscriptionId" -Headers $h -Method Get
Check "GET /subscriptions/{id} -> 200" ($byId.id -eq $subscriptionId)

Write-Host "\n=== TEST 4: duplicate subscription ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions" -Headers $h -Method Post -ContentType "application/json" -Body (JsonBody @{ startDate = $start.ToString("o"); endDate = $end.ToString("o") })
}
Check "second subscription for same organization -> 409 (got $code)" ($code -eq 409)

Write-Host "\n=== TEST 5: invalid lifecycle transitions while Active ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions/$subscriptionId/renew" -Headers $h -Method Put -ContentType "application/json" -Body (JsonBody @{ newEndDate = [DateTime]::UtcNow.AddDays(60).ToString("o") })
}
Check "renew active subscription -> 409 (got $code)" ($code -eq 409)

$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions/$subscriptionId/expire" -Headers $h -Method Put
}
Check "expire active subscription before end date -> 409 (got $code)" ($code -eq 409)

Write-Host "\n=== TEST 6: cancel subscription ==="
$cancelled = Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions/$subscriptionId/cancel" -Headers $h -Method Put
Check "PUT /cancel -> 200" ($cancelled.id -eq $subscriptionId)
Check "subscription becomes Cancelled" ($cancelled.status -eq "Cancelled")

$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions/$subscriptionId/cancel" -Headers $h -Method Put
}
Check "cancel already-cancelled subscription -> 409 (got $code)" ($code -eq 409)

Write-Host "\n=== TEST 7: unauthorized ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/subscriptions" -Method Get
}
Check "GET /subscriptions without JWT -> 401 (got $code)" ($code -eq 401)

Write-Host ""
if ($script:failures -eq 0) { Write-Host "ALL SUBSCRIPTION TESTS PASSED" -ForegroundColor Green }
else { Write-Host "$($script:failures) SUBSCRIPTION TEST(S) FAILED" -ForegroundColor Red; exit 1 }
