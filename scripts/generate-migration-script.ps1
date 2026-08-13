param(
    [string]$Output = 'artifacts/migrations/LeanProd.sql'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$outputPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $Output))
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot 'artifacts'))

if (-not $outputPath.StartsWith($artifactsRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Migration scripts must be generated inside the repository artifacts directory.'
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputPath) | Out-Null
Push-Location $repoRoot
try {
    & dotnet ef migrations script --idempotent `
        --project LeanProd.Infrastructure `
        --startup-project LeanProd.Api `
        --context LeanProdDbContext `
        --output $outputPath

    if ($LASTEXITCODE -ne 0) { throw "dotnet ef exited with code $LASTEXITCODE." }
    Write-Host "Generated: $outputPath"
}
finally {
    Pop-Location
}
