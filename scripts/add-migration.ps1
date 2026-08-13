param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]*$')]
    [string]$Name
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    & dotnet ef migrations add $Name `
        --project LeanProd.Infrastructure `
        --startup-project LeanProd.Api `
        --context LeanProdDbContext `
        --output-dir Common/Persistence/Migrations

    if ($LASTEXITCODE -ne 0) { throw "dotnet ef exited with code $LASTEXITCODE." }
}
finally {
    Pop-Location
}
