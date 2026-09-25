$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    & dotnet build LeanProd.sln --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build exited with code $LASTEXITCODE." }

    foreach ($context in @('LeanProdDbContext', 'IdentityDbContext')) {
        & dotnet ef migrations has-pending-model-changes `
            --project LeanProd.Infrastructure `
            --startup-project LeanProd.Infrastructure `
            --context $context `
            --no-build
        if ($LASTEXITCODE -ne 0) {
            throw "$context differs from its latest migration. Add a migration before committing."
        }
    }
}
finally { Pop-Location }
