# Smoke tests for Profiles bounded context.
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
        firstName = "Profile"
        lastName = "Tester"
        email = "profile.$suffix@resq.com"
        username = $username
        password = $password
        organizationName = "$prefix Organization $suffix"
    }
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-up" -Method Post -ContentType "application/json" -Body (JsonBody $body)
    $signIn = Invoke-RestMethod -Uri "$BaseUrl/api/v1/authentication/sign-in" -Method Post -ContentType "application/json" -Body (JsonBody @{ username = $username; password = $password })
    return $signIn
}

Write-Host "=== SETUP: unique profile test users ==="
$userA = New-TestUser "profile-a"
$userB = New-TestUser "profile-b"
$hA = @{ Authorization = "Bearer $($userA.token)" }

Write-Host "\n=== TEST 1: get current profile ==="
$profile = Invoke-RestMethod -Uri "$BaseUrl/api/v1/profiles/me" -Headers $hA -Method Get
Check "GET /profiles/me -> 200" ($profile.id -eq $userA.id)
Check "profile full name is populated" ($profile.fullName -eq "Profile Tester")
Check "profile email is populated" (-not [string]::IsNullOrWhiteSpace($profile.email))

Write-Host "\n=== TEST 2: get profile by id ==="
$profileById = Invoke-RestMethod -Uri "$BaseUrl/api/v1/profiles/$($userA.id)" -Headers $hA -Method Get
Check "GET /profiles/{id} -> 200" ($profileById.id -eq $userA.id)
Check "profile by id matches /me" ($profileById.email -eq $profile.email)

Write-Host "\n=== TEST 3: update contact information ==="
$updatedEmail = "profile.updated.$([Guid]::NewGuid().ToString('N').Substring(0,8))@resq.com"
$updatedPhone = "987654321"
$updated = Invoke-RestMethod -Uri "$BaseUrl/api/v1/profiles/contact-info" -Headers $hA -Method Put -ContentType "application/json" -Body (JsonBody @{ email = $updatedEmail; phoneNumber = $updatedPhone })
Check "PUT /profiles/contact-info -> 200" ($updated.id -eq $userA.id)
Check "email was updated" ($updated.email -eq $updatedEmail)
Check "phone number was updated" ($updated.phoneNumber -eq $updatedPhone)

Write-Host "\n=== TEST 4: invalid contact information ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/profiles/contact-info" -Headers $hA -Method Put -ContentType "application/json" -Body (JsonBody @{ email = "not-an-email"; phoneNumber = "123" })
}
Check "invalid email -> 400 (got $code)" ($code -eq 400)

Write-Host "\n=== TEST 5: organization isolation ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/profiles/$($userB.id)" -Headers $hA -Method Get
}
Check "profile from another organization -> 404 (got $code)" ($code -eq 404)

Write-Host "\n=== TEST 6: unauthorized ==="
$code = Get-StatusCode {
    Invoke-RestMethod -Uri "$BaseUrl/api/v1/profiles/me" -Method Get
}
Check "GET /profiles/me without JWT -> 401 (got $code)" ($code -eq 401)

Write-Host ""
if ($script:failures -eq 0) { Write-Host "ALL PROFILES TESTS PASSED" -ForegroundColor Green }
else { Write-Host "$($script:failures) PROFILE TEST(S) FAILED" -ForegroundColor Red; exit 1 }
