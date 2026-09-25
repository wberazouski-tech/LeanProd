param(
    [ValidateSet('Business', 'Internal')]
    [string]$Store = 'Business',
    [string]$Migration
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$context = if ($Store -eq 'Internal') { 'IdentityDbContext' } else { 'LeanProdDbContext' }
$efArguments = @('ef', 'database', 'update')
if ($Migration) { $efArguments += $Migration }
$efArguments += @(
    '--project', 'LeanProd.Infrastructure',
    '--startup-project', 'LeanProd.Infrastructure',
    '--context', $context
)
Push-Location $repoRoot
try {
    & dotnet @efArguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef exited with code $LASTEXITCODE." }
}
finally { Pop-Location }
