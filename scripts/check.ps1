param(
    [ValidateSet('All', 'Backend', 'Frontend')][string]$Scope = 'All',
    [switch]$Integration,
    [switch]$Restore
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$npm = if ($env:OS -eq 'Windows_NT') { 'npm.cmd' } else { 'npm' }
function Invoke-Check {
    param([string]$Program, [string[]]$Arguments)
    Write-Host ("Running: {0} {1}" -f $Program, ($Arguments -join ' '))
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Program failed with exit code $LASTEXITCODE." }
}
if ($Integration -and $Scope -eq 'Frontend') { throw '-Integration requires Backend or All scope.' }
Push-Location $root
try {
    # Local agent skills are checked separately with scripts/check-skills.ps1.
    if ($Scope -in @('All', 'Backend')) {
        if ($Integration) { Invoke-Check docker @('info', '--format', '{{.OSType}}') }
        Invoke-Check dotnet @('tool', 'restore')
        Invoke-Check dotnet @('restore', 'LeanProd.sln')
        Invoke-Check dotnet @('build', 'LeanProd.sln', '-c', 'Release', '--no-restore', '--warnaserror')
        # Compare the model without selecting the user's database or applying migrations.
        $names = @('ASPNETCORE_ENVIRONMENT', 'DOTNET_ENVIRONMENT', 'Database__Provider',
            'Database__LocalSettingsPath', 'Database__ApplyMigrationsOnStartup', 'ConnectionStrings__DefaultConnection')
        $saved = @{}
        foreach ($name in $names) { $saved[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
        $validationPath = Join-Path $root ('artifacts/check-' + [guid]::NewGuid().ToString('N'))
        try {
            $env:ASPNETCORE_ENVIRONMENT = 'Development'
            $env:DOTNET_ENVIRONMENT = 'Development'
            $env:Database__Provider = 'SqlServer'
            $env:Database__LocalSettingsPath = $validationPath
            $env:Database__ApplyMigrationsOnStartup = 'false'
            $env:ConnectionStrings__DefaultConnection = 'Server=localhost;Database=LeanProd_ModelValidation;Integrated Security=True;Connect Timeout=1'
            foreach ($context in @('LeanProdDbContext', 'IdentityDbContext')) {
                Invoke-Check dotnet @('ef', 'migrations', 'has-pending-model-changes', '--project',
                    'LeanProd.Infrastructure', '--startup-project', 'LeanProd.Infrastructure', '--context',
                    $context, '--no-build', '--configuration', 'Release')
            }
        }
        finally {
            foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $saved[$name], 'Process') }
            if (Test-Path -LiteralPath $validationPath) {
                if (@(Get-ChildItem -LiteralPath $validationPath -Force).Count -eq 0) {
                    Remove-Item -LiteralPath $validationPath
                }
            }
        }
        Invoke-Check dotnet @('test', 'tests/LeanProd.Domain.UnitTests/LeanProd.Domain.UnitTests.csproj',
            '-c', 'Release', '--no-build', '--logger', 'trx')
        if ($Integration) {
            Invoke-Check dotnet @('test', 'tests/LeanProd.Api.IntegrationTests/LeanProd.Api.IntegrationTests.csproj',
                '-c', 'Release', '--no-build', '--logger', 'trx', '--collect:XPlat Code Coverage')
        }
        else { Write-Host 'NOT RUN: SQL Server Testcontainers tests (use -Integration with Docker, or CI).' }
    }
    if ($Scope -in @('All', 'Frontend')) {
        Push-Location (Join-Path $root 'lean-prod-web')
        try {
            if ($Restore -or !(Test-Path 'node_modules/.bin/ng')) { Invoke-Check $npm @('ci', '--no-fund', '--no-audit') }
            Invoke-Check $npm @('run', 'lint')
            Invoke-Check $npm @('run', 'test:i18n')
            Invoke-Check $npm @('run', 'test:ci')
            Invoke-Check $npm @('run', 'build')
        }
        finally { Pop-Location }
    }
    Write-Host "Checks passed: $Scope; integration requested: $Integration."
}
finally { Pop-Location }
