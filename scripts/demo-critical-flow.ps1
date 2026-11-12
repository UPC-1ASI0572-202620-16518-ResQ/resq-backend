param(
    [Parameter(Mandatory = $true)][string]$ApiBase,
    [Parameter(Mandatory = $true)][string]$Token,
    [Parameter(Mandatory = $true)][Guid]$ZoneId,
    [Parameter(Mandatory = $true)][Guid]$DeviceId,
    [string]$VariableType = "GAS_PPM",
    [string]$Unit = "PPM",
    [decimal]$CriticalThreshold = 500,
    [decimal]$TriggerValue = 750,
    [decimal]$SafeValue = 20
)

$ErrorActionPreference = "Stop"
$headers = @{ Authorization = "Bearer $Token" }
$jsonHeaders = @{ Authorization = "Bearer $Token"; "Content-Type" = "application/json" }

function Invoke-ResQ {
    param([string]$Method, [string]$Path, [object]$Body)
    $parameters = @{ Method = $Method; Uri = "$($ApiBase.TrimEnd('/'))$Path"; Headers = $headers }
    if ($null -ne $Body) {
        $parameters.Headers = $jsonHeaders
        $parameters.Body = $Body | ConvertTo-Json -Depth 12
    }
    Invoke-RestMethod @parameters
}

Write-Host "1. Configuring an effective CRITICAL zone rule"
Invoke-ResQ PUT "/api/v1/monitoring/zones/$ZoneId/profile" @{
    rules = @(@{
        riskTypeCode = "GAS_LEAK"; severityCode = "CRITICAL"; variableType = $VariableType
        operator = "GreaterThanOrEqual"; threshold = $CriticalThreshold
    })
} | Out-Null

Write-Host "2. Registering heartbeat and critical measurement"
Invoke-ResQ POST "/api/v1/connectivity/heartbeats" @{ sensorId = $DeviceId; signalStrength = -55 } | Out-Null
$critical = Invoke-ResQ POST "/api/v1/monitoring/measurements" @{
    deviceId = $DeviceId; variableType = $VariableType; value = $TriggerValue
    unit = $Unit; measuredAt = [DateTimeOffset]::UtcNow.ToString("o")
}

$incidentId = @($critical.generatedIncidentIds)[0]
if (-not $incidentId) { throw "The critical measurement did not generate an incident." }
Write-Host "Generated incident: $incidentId"

Write-Host "3. Assuming incident and recording a human action"
Invoke-ResQ PUT "/api/v1/incidents/$incidentId/assign/me" @{} | Out-Null
Invoke-ResQ POST "/api/v1/incidents/$incidentId/actions" @{
    actionCode = "ZONE_INSPECTED"; notes = "Demo operator inspected and secured the affected zone."
} | Out-Null

Write-Host "4. Sending fresh safe evidence and verifying on the server"
Invoke-ResQ POST "/api/v1/connectivity/heartbeats" @{ sensorId = $DeviceId; signalStrength = -52 } | Out-Null
Invoke-ResQ POST "/api/v1/monitoring/measurements" @{
    deviceId = $DeviceId; variableType = $VariableType; value = $SafeValue
    unit = $Unit; measuredAt = [DateTimeOffset]::UtcNow.ToString("o")
} | Out-Null
$verification = Invoke-ResQ POST "/api/v1/incidents/$incidentId/verify-safe" @{}
if (-not $verification.safe) { throw "Server safety verification failed: $($verification.reason)" }

Write-Host "5. Resolving and closing"
Invoke-ResQ PUT "/api/v1/incidents/$incidentId/resolve" @{
    resolutionNotes = "Fresh telemetry and connectivity verified safe; response actions completed."
} | Out-Null
$closed = Invoke-ResQ PUT "/api/v1/incidents/$incidentId/status" @{ status = "Closed" }

Write-Host "Demo completed: $($closed.id) -> $($closed.status)"
$closed
