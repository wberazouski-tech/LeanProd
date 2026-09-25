using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace LeanProd.Infrastructure.Common.Persistence;

public static class InternalDatabaseConnection
{
    public static string Create(IConfiguration configuration)
    {
        var configuredPath = configuration["InternalDatabase:Path"];
        var databasePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LeanProd",
                "leanprod-internal.db")
            : configuredPath.Trim();

        if (!Path.IsPathFullyQualified(databasePath))
            databasePath = Path.GetFullPath(databasePath, AppContext.BaseDirectory);

        var directory = Path.GetDirectoryName(databasePath)
            ?? throw new InvalidOperationException("Internal database path must include a directory.");
        Directory.CreateDirectory(directory);

        return new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();
    }
}