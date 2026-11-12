# Smoke tests for Incident Management.
# Requires the ResQ API running.

param(
    [string]$BaseUrl = "http://localhost:5058"
)

$ErrorActionPreference = "Stop"
$script:failures = 0

function Check($name, $condition) {
    if ($condition) {
        Write-Host "  [PASS] $name" -ForegroundColor Green
    }
    else {
        Write-Host "  [FAIL] $name" -ForegroundColor Red
        $script:failures++
    }
}

function Get-StatusCode($scriptBlock) {
    try {
        & $scriptBlock | Out-Null
        return 200
    }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            try {
                return [int]$_.Exception.Response.StatusCode.value__
            }
            catch {}

            try {
                return [int]$_.Exception.Response.StatusCode
            }
            catch {}
        }

        return -1
    }
}

function JsonBody($body) {
    return ($body | ConvertTo-Json -Depth 20)
}

function New-TestUser {
    $suffix = [Guid]::NewGuid().ToString("N").Substring(0, 8)

    $username = "incident-$suffix"
    $password = "Password123!"

    $body = @{
        firstName       = "Incident"
        lastName        = "Tester"
        email           = "incident.$suffix@resq.com"
        username        = $username
        password        = $password
        organizationName = "Incident Organization $suffix"
    }

    $null = Invoke-RestMethod `
        -Uri "$BaseUrl/api/v1/authentication/sign-up" `
        -Method Post `
        -ContentType "application/json" `
        -Body (JsonBody $body)

    return Invoke-RestMethod `
        -Uri "$BaseUrl/api/v1/authentication/sign-in" `
        -Method Post `
        -ContentType "application/json" `
        -Body (JsonBody @{
            username = $username
            password = $password
        })
}

Write-Host "=== SETUP: authenticated admin, building and zone ==="

$user = New-TestUser
$h = @{
    Authorization = "Bearer $($user.token)"
}

Write-Host "  Authenticated User.Id = $($user.id)"

$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()

$building = Invoke-RestMethod `
    -Uri "$BaseUrl/api/v1/buildings" `
    -Headers $h `
    -Method Post `
    -ContentType "application/json" `
    -Body (JsonBody @{
        buildingCode = "BLD-INC-$ts"
        name = "Incident Smoke Building"
        description = "Building for Incident Management smoke tests"
        address = @{
            streetAddress = "Av. Arequipa 1200"
            district      = "Lince"
            city          = "Lima"
            countryCode   = "PE"
        }
    })

$buildingWithZone = Invoke-RestMethod `
    -Uri "$BaseUrl/api/v1/buildings/$($building.id)/zones" `
    -Headers $h `
    -Method Post `
    -ContentType "application/json" `
    -Body (JsonBody @{
        zoneCode    = "ZON-INC-$ts"
        name        = "Incident Test Zone"
        description = "Zone for Incident Management smoke tests"
        floorLabel  = "Floor 1"
    })

$zone = @(
    $buildingWithZone.zones |
        Where-Object { $_.zoneCode -eq "ZON-INC-$ts" }
)[0]

Write-Host "`n=== TEST 1: create incident ==="

$incident = Invoke-RestMethod `
    -Uri "$BaseUrl/api/v1/incidents" `
    -Headers $h `
    -Method Post `
    -ContentType "application/json" `
    -Body (JsonBody @{
        zoneId = $zone.id
        type   = "Fire"
        level  = "High"
    })

Check "POST /incidents -> 201 payload" ($incident.id -ne [Guid]::Empty)
Check "new incident starts Active" ($incident.status -eq "Active")
Check "incident belongs to created zone" ($incident.zoneId -eq $zone.id)

$incidentId = $incident.id

Write-Host "`n=== TEST 2: get and list incidents ==="

$byId = Invoke-RestMethod `
    -Uri "$BaseUrl/api/v1/incidents/$incidentId" `
    -Headers $h `
    -Method Get

Check "GET /incidents/{id} -> 200" ($byId.id -eq $incidentId)

$history = @(
    Invoke-RestMethod `
        -Uri "$BaseUrl/api/v1/incidents?zoneId=$($zone.id)" `
        -Headers $h `
        -Method Get
)

Check "GET /incidents -> 200" ($history.Count -ge 1)

Check `
    "zone filter contains created incident" `
    (@($history | Where-Object { $_.id -eq $incidentId }).Count -eq 1)

Write-Host "`n=== TEST 3: assign attendant ==="

# IAM User.Id is INT, and Incident Management now uses INT for assignedTo.
$attendantId = [int]$user.id

Write-Host "  Using IAM User.Id = $attendantId"

$assigned = Invoke-RestMethod `
    -Uri "$BaseUrl/api/v1/incidents/$incidentId/assign" `
    -Headers $h `
    -Method Put `
    -ContentType "application/json" `
    -Body (JsonBody @{
        attendantId = $attendantId
    })

Check "PUT /assign -> 200" ($assigned.id -eq $incidentId)

Check `
    "assignment moves incident to InProgress" `
    ($assigned.status -eq "InProgress")

Check `
    "assigned attendant is the IAM User.Id sent" `
    ([int]$assigned.assignedTo -eq $attendantId)

Write-Host "`n=== TEST 4: resolve incident ==="

Write-Host "  Incident ID: $incidentId"
Write-Host "  Current status before resolve: $($assigned.status)"

$resolveBody = @{
    resolutionNotes = "Se verificó la zona afectada y se controló la situación."
}

Write-Host "  Request body:"
Write-Host (JsonBody $resolveBody)

try {

    $resolved = Invoke-RestMethod `
        -Uri "$BaseUrl/api/v1/incidents/$incidentId/resolve" `
        -Headers $h `
        -Method Put `
        -ContentType "application/json" `
        -Body (JsonBody $resolveBody)

    Check "PUT /resolve -> 200" ($resolved.id -eq $incidentId)

    Check `
        "resolved incident status is Resolved" `
        ($resolved.status -eq "Resolved")

    Check `
        "resolution notes are stored" `
        (-not [string]::IsNullOrWhiteSpace($resolved.resolutionNotes))

    Check `
        "resolvedAt is populated" `
        ($null -ne $resolved.resolvedAt)

}
catch {

    Write-Host "  [FAIL] PUT /resolve -> backend returned an unexpected error" `
        -ForegroundColor Red

    $script:failures++

    if ($_.ErrorDetails.Message) {
        try {
            $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json

            Write-Host "  Backend response:" -ForegroundColor Yellow

            if ($errorBody.errors) {
                foreach ($property in $errorBody.errors.PSObject.Properties) {
                    foreach ($message in $property.Value) {
                        Write-Host "    $($property.Name): $message" `
                            -ForegroundColor Yellow
                    }
                }
            }
            elseif ($errorBody.message) {
                Write-Host "    $($errorBody.message)" `
                    -ForegroundColor Yellow
            }
            else {
                Write-Host "    $($_.ErrorDetails.Message)" `
                    -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "  Raw response:" -ForegroundColor Yellow
            Write-Host "    $($_.ErrorDetails.Message)" -ForegroundColor Yellow
        }
    }
}

# Only continue with resolve-dependent tests if the resolve request succeeded.
if ($resolved) {

    Write-Host "`n=== TEST 5: close incident ==="

    $closed = Invoke-RestMethod `
        -Uri "$BaseUrl/api/v1/incidents/$incidentId/status" `
        -Headers $h `
        -Method Put `
        -ContentType "application/json" `
        -Body (JsonBody @{
            status = "Closed"
        })

    Check "PUT /status -> 200" ($closed.id -eq $incidentId)

    Check `
        "incident reaches Closed" `
        ($closed.status -eq "Closed")

    Write-Host "`n=== TEST 6: validation and business rules ==="

    $code = Get-StatusCode {
        Invoke-RestMethod `
            -Uri "$BaseUrl/api/v1/incidents/$incidentId/resolve" `
            -Headers $h `
            -Method Put `
            -ContentType "application/json" `
            -Body (JsonBody @{
                resolutionNotes = ""
            })
    }

    Check `
        "resolving closed incident -> 409 (got $code)" `
        ($code -eq 409)

    $code = Get-StatusCode {
        Invoke-RestMethod `
            -Uri "$BaseUrl/api/v1/incidents/$incidentId/status" `
            -Headers $h `
            -Method Put `
            -ContentType "application/json" `
            -Body (JsonBody @{
                status = "NotAStatus"
            })
    }

    Check `
        "invalid status -> 400 (got $code)" `
        ($code -eq 400)

    $code = Get-StatusCode {
        Invoke-RestMethod `
            -Uri "$BaseUrl/api/v1/incidents/$([Guid]::NewGuid())" `
            -Headers $h `
            -Method Get
    }

    Check `
        "unknown incident -> 404 (got $code)" `
        ($code -eq 404)

}
else {

    Write-Host "`n=== TEST 5: close incident ==="
    Write-Host "  [SKIP] Resolve failed, so close test was skipped." `
        -ForegroundColor Yellow

    Write-Host "`n=== TEST 6: validation and business rules ==="
    Write-Host "  [SKIP] Resolve-dependent validation tests were skipped." `
        -ForegroundColor Yellow
}

Write-Host "`n=== TEST 7: unauthorized ==="

$code = Get-StatusCode {
    Invoke-RestMethod `
        -Uri "$BaseUrl/api/v1/incidents" `
        -Method Get
}

Check `
    "GET /incidents without JWT -> 401 (got $code)" `
    ($code -eq 401)

Write-Host ""

if ($script:failures -eq 0) {

    Write-Host "ALL INCIDENT TESTS PASSED" `
        -ForegroundColor Green

}
else {

    Write-Host "$($script:failures) INCIDENT TEST(S) FAILED" `
        -ForegroundColor Red

    exit 1
}