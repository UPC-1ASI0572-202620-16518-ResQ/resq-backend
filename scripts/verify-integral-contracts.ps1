$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

dotnet build (Join-Path $root "ResQ-Backend.sln") --no-restore
if ($LASTEXITCODE -ne 0) { throw "Backend build failed." }

$checks = @(
    @{ File = "ResQ.API/Incident-Management/Interfaces/REST/IncidentsController.cs"; Text = 'HttpPut("{id:guid}/assign/me")' },
    @{ File = "ResQ.API/Incident-Management/Interfaces/REST/IncidentsController.cs"; Text = 'HttpPost("{id:guid}/verify-safe")' },
    @{ File = "ResQ.API/Monitoring/Interfaces/REST/ZoneMonitoringProfilesController.cs"; Text = '[HttpPut]' },
    @{ File = "ResQ.API/Monitoring/Interfaces/REST/MonitoringController.cs"; Text = 'measurements/history' },
    @{ File = "ResQ.API/Building-Management/Interfaces/REST/FloorPlansController.cs"; Text = '[HttpPut]' },
    @{ File = "ResQ.API/Risk-Detection/Interfaces/REST/Resources/RiskDetectionResources.cs"; Text = 'GeneratedIncidentIds' }
)

foreach ($check in $checks) {
    $path = Join-Path $root $check.File
    $content = Get-Content -LiteralPath $path -Raw
    if (-not $content.Contains($check.Text)) { throw "Missing contract '$($check.Text)' in $($check.File)." }
}

Write-Host "Integral RESQ contracts verified."
