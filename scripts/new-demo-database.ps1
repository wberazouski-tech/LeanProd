param(
    [string]$Server = '.\SQLEXPRESS',
    [ValidatePattern('^LeanProd_Demo(?:_[A-Za-z0-9_]+)?$')][string]$Database = 'LeanProd_Demo'
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'local-sql.ps1')
$root = Split-Path -Parent $PSScriptRoot
$master = Open-LocalSql $Server
try {
    $existing = Invoke-LocalSql $master 'SELECT name FROM sys.databases WHERE name=@name' @{ '@name' = $Database }
    if ($existing.Rows.Count -gt 0) { throw "$Database already exists. Choose a new LeanProd_Demo_<name>; this script never replaces a database." }
}
finally { $master.Dispose() }
$names = @('ASPNETCORE_ENVIRONMENT','DOTNET_ENVIRONMENT','Database__Provider',
    'Database__LocalSettingsPath','Database__ApplyMigrationsOnStartup','ConnectionStrings__DefaultConnection',
    'BootstrapAdmin__Email','BootstrapAdmin__Password')
$saved = @{}
foreach ($name in $names) { $saved[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
Push-Location $root
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Demo'
    $env:DOTNET_ENVIRONMENT = 'Demo'
    $env:Database__Provider = 'SqlServer'
    $env:Database__LocalSettingsPath = Join-Path $root ('artifacts/demo-provision-' + [guid]::NewGuid().ToString('N'))
    $env:Database__ApplyMigrationsOnStartup = 'false'
    $env:ConnectionStrings__DefaultConnection = "Server=$Server;Database=$Database;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
    $env:BootstrapAdmin__Email = ''
    $env:BootstrapAdmin__Password = ''
    & dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'EF tool restore failed.' }
    & dotnet ef database update --project LeanProd.Infrastructure --startup-project LeanProd.Api --context LeanProdDbContext --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Migration failed; $Database may be partially provisioned. Inspect it; do not overwrite it." }
    $demo = Open-LocalSql $Server $Database
    try {
        $seed = Get-Content (Join-Path $PSScriptRoot 'seed-catalog-demo.sql') -Raw -Encoding UTF8
        $result = Invoke-LocalSql $demo $seed
        $result | Format-Table -AutoSize
        $counts = Invoke-LocalSql $demo 'SELECT (SELECT COUNT(*) FROM __EFMigrationsHistory) AS Migrations, (SELECT COUNT(*) FROM CatalogItems) AS CatalogItems, (SELECT COUNT(*) FROM AspNetUsers) AS Users'
        $counts | Format-Table -AutoSize
    }
    finally { $demo.Dispose() }
    Write-Host "Created $Server / $Database with demo reference data. No users were created."
}
finally {
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $saved[$name], 'Process') }
    Pop-Location
}
