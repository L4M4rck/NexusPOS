$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot

Set-Location -LiteralPath $projectRoot
docker compose up mysql -d

$backendProcess = Start-Process dotnet -ArgumentList @('run', '--project', 'backend/src/NexusPOS.Api') -WorkingDirectory $projectRoot -PassThru -WindowStyle Hidden
try {
    Set-Location -LiteralPath (Join-Path $projectRoot 'frontend')
    npm.cmd run dev
}
finally {
    if (-not $backendProcess.HasExited) {
        Stop-Process -Id $backendProcess.Id
    }
}
