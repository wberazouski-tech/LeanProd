param(
    [string]$Migration
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$efArguments = @(
    'ef', 'database', 'update'
)
if ($Migration) { $efArguments += $Migration }
$efArguments += @(
    '--project', 'LeanProd.Infrastructure',
    '--startup-project', 'LeanProd.Api',
    '--context', 'LeanProdDbContext'
)

Push-Location $repoRoot
try {
    & dotnet @efArguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef exited with code $LASTEXITCODE." }
}
finally {
    Pop-Location
}
