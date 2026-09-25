param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]*$')]
    [string]$Name,
    [ValidateSet('Business', 'Internal')]
    [string]$Store = 'Business'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$context = if ($Store -eq 'Internal') { 'IdentityDbContext' } else { 'LeanProdDbContext' }
$output = "Common/Persistence/Migrations/$Store"

Push-Location $repoRoot
try {
    & dotnet ef migrations add $Name `
        --project LeanProd.Infrastructure `
        --startup-project LeanProd.Infrastructure `
        --context $context `
        --output-dir $output
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef exited with code $LASTEXITCODE." }
}
finally { Pop-Location }
