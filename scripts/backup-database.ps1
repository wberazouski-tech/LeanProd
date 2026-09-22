param(
    [string]$Server = '.\SQLEXPRESS',
    [ValidatePattern('^LeanProd(?:_[A-Za-z0-9_]+)?$')][string]$Database = 'LeanProd'
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'local-sql.ps1')
$connection = Open-LocalSql $Server
try {
    $row = Invoke-LocalSql $connection "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000)) AS Folder; "
    $folder = [string]$row.Rows[0].Folder
    if (!$folder) { throw 'SQL Server did not report its default backup directory.' }
    $exists = Invoke-LocalSql $connection 'SELECT name FROM sys.databases WHERE name=@name' @{ '@name' = $Database }
    if ($exists.Rows.Count -ne 1) { throw "Database $Database does not exist." }
    $file = Join-Path $folder ($Database + '_' + (Get-Date -Format 'yyyyMMdd_HHmmss') + '_' + [guid]::NewGuid().ToString('N').Substring(0,8) + '.bak')
    [void](Invoke-LocalSql $connection "BACKUP DATABASE [$Database] TO DISK=@file WITH COPY_ONLY, CHECKSUM; RESTORE VERIFYONLY FROM DISK=@file WITH CHECKSUM;" @{ '@file' = $file })
    $manifest = [ordered]@{ Server=$Server; Database=$Database; BackupFile=$file; CreatedUtc=[DateTime]::UtcNow.ToString('o'); VerifyOnlyPassed=$true; RestoreTested=$false }
    $root = Split-Path -Parent $PSScriptRoot
    $manifestFolder = Join-Path $root 'artifacts/backups'
    [void][IO.Directory]::CreateDirectory($manifestFolder)
    $manifestPath = Join-Path $manifestFolder ([IO.Path]::GetFileNameWithoutExtension($file) + '.json')
    [IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json), (New-Object Text.UTF8Encoding($false)))
    Write-Host "Backup and VERIFYONLY passed. Manifest: $manifestPath"
    Write-Output $manifestPath
}
finally { $connection.Dispose() }
