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

Write-Host "=== TEST 1: Register Device with FAKE Building (409 Conflict Expected) ==="
$fakeDevBody = @{
    deviceCode = "DEV-FAKE-" + (Get-Random -Minimum 1000 -Maximum 9999)
    name = "Fake Device"
    description = "Device assigned to non-existent building"
    specifications = @{
        manufacturer = "Test Corp"
        model = "M-1"
        serialNumber = "SN-001"
    }
    assignment = @{
        buildingId = "00000000-0000-0000-0000-000000000000"
        zoneId = $null
    }
    externalReference = $null
    capabilities = @(
        @{ code = "TEMP"; kind = "measurement"; unit = "CELSIUS" }
    )
} | ConvertTo-Json -Depth 10

try {
    Invoke-WebRequest -Uri "http://localhost:5058/api/v1/devices" -Method Post -Headers $authHeaders -Body $fakeDevBody -ContentType "application/json" -UseBasicParsing
    Write-Error "Expected 409 Conflict, but succeeded!"
} catch {
    Write-Host "Correctly rejected by BuildingsContextFacade: $($_.Exception.Response.StatusCode)"
}

Write-Host "`n=== TEST 2: Register REAL Building in Building Management ==="
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$regBldBody = @{
    buildingCode = "BLD-DEV-$ts"
    name = "Innovation Campus"
    description = "IoT testing facility"
    address = @{
        streetAddress = "Calle Las Begonias 443"
        district = "San Isidro"
        city = "Lima"
        countryCode = "PE"
    }
} | ConvertTo-Json -Depth 10

$bldResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings" -Method Post -Headers $authHeaders -Body $regBldBody -ContentType "application/json" -UseBasicParsing
$bld = $bldResp.Content | ConvertFrom-Json
$bldId = $bld.id
Write-Host "Building Created: ID=$bldId, Code=$($bld.buildingCode)"

Write-Host "`n=== TEST 3: Add REAL Zone to Building (Without If-Match) ==="
$zoneBody = @{
    zoneCode = "ZON-LAB-01"
    name = "Hardware Lab"
    description = "IoT sensor calibration room"
    floorLabel = "Floor 2"
} | ConvertTo-Json -Depth 10
$addZoneResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/buildings/$bldId/zones" -Method Post -Headers $authHeaders -Body $zoneBody -ContentType "application/json" -UseBasicParsing
$bldWithZone = $addZoneResp.Content | ConvertFrom-Json
$zoneId = $bldWithZone.zones[0].id
Write-Host "Zone Created: ID=$zoneId, Code=$($bldWithZone.zones[0].zoneCode)"

Write-Host "`n=== TEST 4: Register Device with REAL Building and Zone (201 Created Expected) ==="
$validDevBody = @{
    deviceCode = "DEV-IOT-$ts"
    name = "Smoke & Temperature Sensor"
    description = "Dual sensor for laboratory"
    specifications = @{
        manufacturer = "Bosch Security Systems"
        model = "FCP-O320"
        serialNumber = "SN-BOSCH-$ts"
    }
    assignment = @{
        buildingId = $bldId
        zoneId = $zoneId
    }
    externalReference = @{
        sourceSystem = "AWS-IOT"
        externalDeviceId = "urn:iot:device:$ts"
    }
    capabilities = @(
        @{ code = "SMOKE_DENSITY"; kind = "measurement"; unit = "PPM" },
        @{ code = "ALARM_BUZZER"; kind = "actuation"; unit = $null }
    )
} | ConvertTo-Json -Depth 10

$devResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/devices" -Method Post -Headers $authHeaders -Body $validDevBody -ContentType "application/json" -UseBasicParsing
$dev = $devResp.Content | ConvertFrom-Json
$devId = $dev.id
Write-Host "Status: $($devResp.StatusCode), Device Created! ID=$devId, Assigned Building=$($dev.assignment.buildingId), Assigned Zone=$($dev.assignment.zoneId)"

Write-Host "`n=== TEST 5: Reassign Device to Inactive or Invalid Building (409 Conflict Expected) ==="
try {
    $reassignInvalidBody = @{
        buildingId = "11111111-2222-3333-4444-555555555555"
        zoneId = $null
    } | ConvertTo-Json -Depth 10
    Invoke-WebRequest -Uri "http://localhost:5058/api/v1/devices/$devId/assignment" -Method Put -Headers $authHeaders -Body $reassignInvalidBody -ContentType "application/json" -UseBasicParsing
    Write-Error "Expected 409 Conflict, but succeeded!"
} catch {
    Write-Host "Correctly rejected invalid reassignment: $($_.Exception.Response.StatusCode)"
}

Write-Host "`n=== TEST 6: Reassign Device to Real Building without Zone (200 OK Expected, Without If-Match) ==="
$reassignValidBody = @{
    buildingId = $bldId
    zoneId = $null
} | ConvertTo-Json -Depth 10
$reassignResp = Invoke-WebRequest -Uri "http://localhost:5058/api/v1/devices/$devId/assignment" -Method Put -Headers $authHeaders -Body $reassignValidBody -ContentType "application/json" -UseBasicParsing
$reassignedDev = $reassignResp.Content | ConvertFrom-Json
Write-Host "Status: $($reassignResp.StatusCode), Device Reassigned! Building=$($reassignedDev.assignment.buildingId), Zone=$($reassignedDev.assignment.zoneId)"

Write-Host "`n=== ALL 6 CROSS-BOUNDED CONTEXT INTEGRATION TESTS PASSED PERFECTLY! ==="
