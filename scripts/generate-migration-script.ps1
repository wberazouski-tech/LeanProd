param(
    [ValidateSet('Business', 'Internal')]
    [string]$Store = 'Business',
    [string]$Output
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $Output) { $Output = "artifacts/migrations/LeanProd-$Store.sql" }
$outputPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $Output))
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot 'artifacts'))
if (-not $outputPath.StartsWith($artifactsRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Migration scripts must be generated inside the repository artifacts directory.'
}
$context = if ($Store -eq 'Internal') { 'IdentityDbContext' } else { 'LeanProdDbContext' }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputPath) | Out-Null
Push-Location $repoRoot
try {
    & dotnet ef migrations script --idempotent `
        --project LeanProd.Infrastructure `
        --startup-project LeanProd.Infrastructure `
        --context $context `
        --output $outputPath
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef exited with code $LASTEXITCODE." }
    Write-Host "Generated: $outputPath"
}
finally { Pop-Location }
