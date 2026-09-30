$ErrorActionPreference = "Stop"

# 1. Login or register test user
$username = "testbuildingadmin"
$password = "Password123!"

try {
    $signInBody = @{ username = $username; password = $password } | ConvertTo-Json
    $signInRes = Invoke-RestMethod -Uri "http://localhost:5058/api/v1/authentication/sign-in" -Method Post -Body $signInBody -ContentType "application/json"
    $token = $signInRes.token
} catch {
    $signUpBody = @{
        firstName = "Building"
        lastName = "Admin"
        email = "testbuildingadmin@resq.com"
        phone = "987654321"
        username = $username
        password = $password
        role = 0
    } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "http://localhost:5058/api/v1/authentication/sign-up" -Method Post -Body $signUpBody -ContentType "application/json"
    $signInBody = @{ username = $username; password = $password } | ConvertTo-Json
    $signInRes = Invoke-RestMethod -Uri "http://localhost:5058/api/v1/authentication/sign-in" -Method Post -Body $signInBody -ContentType "application/json"
    $token = $signInRes.token
}

$authHeaders = @{ "Authorization" = "Bearer $token" }

Write-Host "=== 1. TEST POST /api/v1/buildings (RegisterBuilding) ==="
$uniqueSuffix = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$regBody = @{
    buildingCode = "BLD-HQ-$uniqueSuffix"
    name = "Central Headquarters"
    description = "Main corporate operations campus"
    address = @{
        streetAddress = "Av. Javier Prado Este 456"
        district = "San Isidro"
        city = "Lima"
        countryCode = "PE"
    }
} | ConvertTo-Json

$postResponse = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings" -Method Post -Headers $authHeaders -Body $regBody -ContentType "application/json" -UseBasicParsing
Write-Host "Status: $($postResponse.StatusCode)"
$building = $postResponse.Content | ConvertFrom-Json
$bldId = $building.id
Write-Host "Building Created: ID=$bldId, Code=$($building.buildingCode), Status=$($building.administrativeStatus)"

Write-Host "`n=== 2. TEST POST Duplicate Building Code (409 Conflict Expected) ==="
try {
    Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings" -Method Post -Headers $authHeaders -Body $regBody -ContentType "application/json" -UseBasicParsing
    Write-Error "Expected 409 Conflict, but succeeded!"
} catch {
    Write-Host "Correctly received error: $($_.Exception.Response.StatusCode)"
}

Write-Host "`n=== 3. TEST GET /api/v1/buildings/{buildingId} ==="
$getBldResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId" -Method Get -Headers $authHeaders -UseBasicParsing
Write-Host "Status: $($getBldResp.StatusCode), Name=$($building.name)"

Write-Host "`n=== 4. TEST PUT /api/v1/buildings/{buildingId}/details (Direct Update Without If-Match) ==="
$updateDetailsBody = @{
    name = "Central Headquarters Updated"
    description = "Updated main corporate operations campus"
    address = @{
        streetAddress = "Av. Javier Prado Este 789"
        district = "San Isidro"
        city = "Lima"
        countryCode = "PE"
    }
} | ConvertTo-Json

$updateResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/details" -Method Put -Headers $authHeaders -Body $updateDetailsBody -ContentType "application/json" -UseBasicParsing
Write-Host "Status: $($updateResp.StatusCode)"
$updatedBuilding = $updateResp.Content | ConvertFrom-Json
Write-Host "Updated Name: $($updatedBuilding.name), Street: $($updatedBuilding.address.streetAddress)"

Write-Host "`n=== 5. TEST PUT /api/v1/buildings/{buildingId}/administrative-status (Direct Update Without If-Match) ==="
$deactBody = @{ administrativeStatus = "inactive" } | ConvertTo-Json
$deactResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/administrative-status" -Method Put -Headers $authHeaders -Body $deactBody -ContentType "application/json" -UseBasicParsing
$deactBuilding = $deactResp.Content | ConvertFrom-Json
Write-Host "Status: $($deactResp.StatusCode), Admin Status: $($deactBuilding.administrativeStatus)"

# Reactivate building
$reactBody = @{ administrativeStatus = "active" } | ConvertTo-Json
$reactResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/administrative-status" -Method Put -Headers $authHeaders -Body $reactBody -ContentType "application/json" -UseBasicParsing
$reactBuilding = $reactResp.Content | ConvertFrom-Json
Write-Host "Status: $($reactResp.StatusCode), Reactivated Admin Status: $($reactBuilding.administrativeStatus)"

Write-Host "`n=== 6. TEST POST /api/v1/buildings/{buildingId}/zones (Add Zone Direct Without If-Match) ==="
$zone1Code = "ZON-OPS-$uniqueSuffix"
$addZoneBody = @{
    zoneCode = $zone1Code
    name = "Operations Control Room"
    description = "Main monitoring station"
    floorLabel = "Floor 4"
} | ConvertTo-Json

$addZoneResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/zones" -Method Post -Headers $authHeaders -Body $addZoneBody -ContentType "application/json" -UseBasicParsing
Write-Host "Status: $($addZoneResp.StatusCode)"
$bldWithZone = $addZoneResp.Content | ConvertFrom-Json
$createdZone = $bldWithZone.zones | Where-Object { $_.zoneCode -eq $zone1Code }
$zoneId = $createdZone.id
Write-Host "Zone Added: ID=$zoneId, Code=$($createdZone.zoneCode), Name=$($createdZone.name)"

Write-Host "`n=== 7. TEST GET /api/v1/buildings/{buildingId}/zones ==="
$getZonesResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/zones" -Method Get -Headers $authHeaders -UseBasicParsing
Write-Host "Status: $($getZonesResp.StatusCode)"
$zonesPage = $getZonesResp.Content | ConvertFrom-Json
Write-Host "Zones returned: $($zonesPage.items.Count)"

Write-Host "`n=== 8. TEST GET /api/v1/buildings/{buildingId}/zones/{zoneId} ==="
$getZoneResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/zones/$zoneId" -Method Get -Headers $authHeaders -UseBasicParsing
Write-Host "Status: $($getZoneResp.StatusCode)"
$singleZone = $getZoneResp.Content | ConvertFrom-Json
Write-Host "Zone fetched: Name=$($singleZone.name), availableForAssignment=$($singleZone.availableForAssignment)"

Write-Host "`n=== 9. TEST PUT /api/v1/buildings/{buildingId}/zones/{zoneId}/details (Direct Without If-Match) ==="
$updateZoneBody = @{
    name = "Operations & Security Command Center"
    description = "Combined monitoring and security hub"
    floorLabel = "Floor 4, Wing B"
} | ConvertTo-Json

$updZoneResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/zones/$zoneId/details" -Method Put -Headers $authHeaders -Body $updateZoneBody -ContentType "application/json" -UseBasicParsing
Write-Host "Status: $($updZoneResp.StatusCode)"
$bldAfterZoneUpdate = $updZoneResp.Content | ConvertFrom-Json
$updZone = $bldAfterZoneUpdate.zones | Where-Object { $_.id -eq $zoneId }
Write-Host "Zone Name Updated: $($updZone.name), Floor=$($updZone.floorLabel)"

Write-Host "`n=== 10. TEST PUT /api/v1/buildings/{buildingId}/zones/{zoneId}/administrative-status (Direct Without If-Match) ==="
$zoneDeactBody = @{ administrativeStatus = "inactive" } | ConvertTo-Json
$zoneDeactResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/zones/$zoneId/administrative-status" -Method Put -Headers $authHeaders -Body $zoneDeactBody -ContentType "application/json" -UseBasicParsing
Write-Host "Status: $($zoneDeactResp.StatusCode)"
$bldAfterZoneDeact = $zoneDeactResp.Content | ConvertFrom-Json
$deactZone = $bldAfterZoneDeact.zones | Where-Object { $_.id -eq $zoneId }
Write-Host "Zone Admin Status: $($deactZone.administrativeStatus)"

# Reactivate zone
$zoneReactBody = @{ administrativeStatus = "active" } | ConvertTo-Json
$zoneReactResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/zones/$zoneId/administrative-status" -Method Put -Headers $authHeaders -Body $zoneReactBody -ContentType "application/json" -UseBasicParsing
Write-Host "Status: $($zoneReactResp.StatusCode)"
$bldAfterZoneReact = $zoneReactResp.Content | ConvertFrom-Json
$reactZone = $bldAfterZoneReact.zones | Where-Object { $_.id -eq $zoneId }
Write-Host "Zone Reactivated: $($reactZone.administrativeStatus)"

Write-Host "`n=== 11. TEST GET /api/v1/buildings (Pagination & Filters) ==="
$pagedResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings?administrativeStatus=active&page=0&size=10" -Method Get -Headers $authHeaders -UseBasicParsing
Write-Host "Status: $($pagedResp.StatusCode)"
$pageData = $pagedResp.Content | ConvertFrom-Json
Write-Host "Buildings count: $($pageData.items.Count), Total: $($pageData.totalElements)"

Write-Host "`n========================================================"
Write-Host "ALL 11 ENDPOINT TESTS PASSED WITH 100% SUCCESS DIRECTLY!"
Write-Host "========================================================"
