# Shared Windows PowerShell helpers; no credentials are written or printed.
function Open-LocalSql {
    param([string]$Server, [string]$Database = 'master')
    if ($Server -notmatch '^(?:\.|localhost)(?:\\[A-Za-z0-9_]+)?$') { throw 'Only a local SQL Server instance is allowed.' }
    if ($Database -notmatch '^(master|LeanProd(?:_[A-Za-z0-9_]+)?)$') { throw 'Unexpected database name.' }
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = $Server
    $builder['Initial Catalog'] = $Database
    $builder['Integrated Security'] = $true
    $builder['Encrypt'] = $true
    $builder['TrustServerCertificate'] = $true
    $connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
    $connection.Open()
    return $connection
}
function Invoke-LocalSql {
    param($Connection, [string]$Sql, [hashtable]$Parameters = @{})
    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $Sql
        $command.CommandTimeout = 300
        foreach ($name in $Parameters.Keys) { [void]$command.Parameters.AddWithValue($name, $Parameters[$name]) }
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        try {
            $table = New-Object System.Data.DataTable
            [void]$adapter.Fill($table)
            return ,$table
        }
        finally { $adapter.Dispose() }
    }
    finally { $command.Dispose() }
}
function Quote-SqlLiteral([string]$Value) { return "N'" + $Value.Replace("'", "''") + "'" }
