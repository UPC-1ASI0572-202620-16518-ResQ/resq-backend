# Smoke tests for IAM / Authentication.
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

function JsonBody($body) {
    return ($body | ConvertTo-Json -Depth 20)
}

Write-Host "=== SETUP: unique IAM test user ==="
$suffix = [Guid]::NewGuid().ToString("N").Substring(0, 8)
$username = "iamtest-$suffix"
$password = "Password123!"
$email = "iam.$suffix@resq.com"
$organizationName = "IAM Smoke Organization $suffix"

$signUpBody = @{
    firstName = "IAM"
    lastName = "SmokeTester"
    email = $email
    username = $username
    password = $password
    organizationName = $organizationName
}

$signUp = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -ContentType "application/json" -Body (JsonBody $signUpBody)
Check "sign-up returns success" ($signUp.message -eq "User created successfully")

$signInBody = @{ username = $username; password = $password }
$signIn = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -ContentType "application/json" -Body (JsonBody $signInBody)
$token = $signIn.token

Write-Host "\n=== TEST 1: successful authentication ==="
Check "sign-in returns JWT" (-not [string]::IsNullOrWhiteSpace($token))
Check "authenticated user id is valid" ([int]$signIn.id -gt 0)
Check "new sign-up user is Admin" ($signIn.role -eq "Admin")
Check "organization id is present" ([Guid]$signIn.organizationId -ne [Guid]::Empty)

Write-Host "\n=== TEST 2: invalid credentials ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -ContentType "application/json" -Body (JsonBody @{ username = $username; password = "WrongPassword123!" })
}
Check "wrong password -> 401 (got $code)" ($code -eq 401)

Write-Host "\n=== TEST 3: duplicate username ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -ContentType "application/json" -Body (JsonBody $signUpBody)
}
Check "duplicate username -> 409 (got $code)" ($code -eq 409)

Write-Host "\n=== TEST 4: registration validation ==="
$invalidSuffix = [Guid]::NewGuid().ToString("N").Substring(0, 8)
$invalidSignUp = @{
    firstName = "Invalid"
    lastName = "Password"
    email = "invalid.$invalidSuffix@resq.com"
    username = "invalid-$invalidSuffix"
    password = "abc"
    organizationName = "Invalid Organization $invalidSuffix"
}
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -ContentType "application/json" -Body (JsonBody $invalidSignUp)
}
Check "weak password -> 400 (got $code)" ($code -eq 400)

Write-Host "\n=== TEST 5: protected endpoint without JWT ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/profiles/me" -Method Get
}
Check "GET protected profile without token -> 401 (got $code)" ($code -eq 401)

Write-Host ""
if ($script:failures -eq 0) { Write-Host "ALL IAM TESTS PASSED" -ForegroundColor Green }
else { Write-Host "$($script:failures) IAM TEST(S) FAILED" -ForegroundColor Red; exit 1 }
