param(
    [Parameter(Mandatory=$true)][string]$Manifest,
    [ValidatePattern('^LeanProd_RestoreCheck_[A-Za-z0-9_]+$')]
    [string]$Database = ('LeanProd_RestoreCheck_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'local-sql.ps1')
$info = Get-Content -LiteralPath $Manifest -Raw -Encoding UTF8 | ConvertFrom-Json
$connection = Open-LocalSql $info.Server
try {
    $exists = Invoke-LocalSql $connection 'SELECT name FROM sys.databases WHERE name=@name' @{ '@name' = $Database }
    if ($exists.Rows.Count -gt 0) { throw "$Database already exists. Restore verification never overwrites databases." }
    $paths = Invoke-LocalSql $connection "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000)) AS DataPath, CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(4000)) AS LogPath"
    if (!$paths.Rows[0].DataPath -or !$paths.Rows[0].LogPath) { throw 'Default SQL data paths are missing.' }
    $files = Invoke-LocalSql $connection 'RESTORE FILELISTONLY FROM DISK=@file' @{ '@file' = $info.BackupFile }
    $moves = @()
    $index = 0
    foreach ($file in $files.Rows) {
        if ($file.Type -notin @('D','L')) { throw 'This helper only supports data and log files.' }
        $folder = if ($file.Type -eq 'L') { $paths.Rows[0].LogPath } else { $paths.Rows[0].DataPath }
        $extension = if ($file.Type -eq 'L') { '.ldf' } elseif ($index -eq 0) { '.mdf' } else { '.ndf' }
        $destination = Join-Path $folder ($Database + '_' + $index + $extension)
        $moves += ('MOVE ' + (Quote-SqlLiteral $file.LogicalName) + ' TO ' + (Quote-SqlLiteral $destination))
        $index++
    }
    [void](Invoke-LocalSql $connection ("RESTORE DATABASE [$Database] FROM DISK=@file WITH CHECKSUM, " + ($moves -join ', ')) @{ '@file' = $info.BackupFile })
}
finally { $connection.Dispose() }
$restored = Open-LocalSql $info.Server $Database
try {
    $errors = Invoke-LocalSql $restored "DBCC CHECKDB ([$Database]) WITH NO_INFOMSGS, TABLERESULTS"
    if ($errors.Rows.Count -gt 0) { throw 'DBCC CHECKDB returned errors.' }
    $counts = Invoke-LocalSql $restored 'SELECT (SELECT COUNT(*) FROM __EFMigrationsHistory) AS Migrations, (SELECT COUNT(*) FROM CatalogItems) AS CatalogItems, (SELECT COUNT(*) FROM AspNetUsers) AS Users'
    if ([int]$counts.Rows[0].Migrations -lt 1) { throw 'Restored database has no LeanProd migrations.' }
    $counts | Format-Table -AutoSize
    $info.RestoreTested = $true
    $info | Add-Member -NotePropertyName RestoredDatabase -NotePropertyValue $Database -Force
    $info | Add-Member -NotePropertyName RestoreCheckedUtc -NotePropertyValue ([DateTime]::UtcNow.ToString('o')) -Force
    [IO.File]::WriteAllText((Resolve-Path -LiteralPath $Manifest).Path, ($info | ConvertTo-Json), (New-Object Text.UTF8Encoding($false)))
    Write-Host "Restore and DBCC CHECKDB passed: $Database. Verification database retained; no databases were deleted."
}
finally { $restored.Dispose() }
