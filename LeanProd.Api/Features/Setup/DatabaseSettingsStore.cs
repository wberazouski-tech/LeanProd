using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LeanProd.Application.Common.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LeanProd.Api.Features.Setup;

public interface IDatabaseSettingsStore : IRuntimeDatabaseConnection
{
    string SettingsPath { get; }
    DatabaseRuntimeSettings? Load();
    void Save(string provider, string connectionString, string server, string database, string appLogin);
}

public sealed record DatabaseRuntimeSettings(
    string Provider,
    string ConnectionString,
    string Server,
    string Database,
    string AppLogin);

public sealed class DatabaseSettingsStore : IDatabaseSettingsStore
{
    private const string DefaultSettingsFileName = "database-settings.json";
    private readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
    private readonly IConfiguration configuration;

    public DatabaseSettingsStore(IConfiguration configuration)
    {
        this.configuration = configuration;
        var configuredFolder = configuration["Database:LocalSettingsPath"];
        var folder = string.IsNullOrWhiteSpace(configuredFolder)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LeanProd")
            : configuredFolder;
        Directory.CreateDirectory(folder);
        SettingsPath = Path.Combine(folder, DefaultSettingsFileName);
    }

    public string SettingsPath { get; }
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);
    public string Provider => Load()?.Provider ?? configuration["Database:Provider"] ?? "SqlServer";
    public string ConnectionString => Load()?.ConnectionString
        ?? configuration.GetConnectionString("DefaultConnection")
        ?? string.Empty;

    public DatabaseRuntimeSettings? Load()
    {
        if (!File.Exists(SettingsPath)) return null;
        var persisted = JsonSerializer.Deserialize<PersistedDatabaseSettings>(
            File.ReadAllText(SettingsPath, Encoding.UTF8), jsonOptions);
        if (persisted is null) return null;

        var password = Unprotect(persisted.EncryptedPassword);
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = persisted.Server,
            InitialCatalog = persisted.Database,
            UserID = persisted.AppLogin,
            Password = password,
            Encrypt = persisted.Encrypt,
            TrustServerCertificate = persisted.TrustServerCertificate,
            MultipleActiveResultSets = true
        };
        return new DatabaseRuntimeSettings(
            persisted.Provider,
            builder.ConnectionString,
            persisted.Server,
            persisted.Database,
            persisted.AppLogin);
    }

    public void Save(string provider, string connectionString, string server, string database, string appLogin)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var persisted = new PersistedDatabaseSettings
        {
            Provider = provider,
            Server = server,
            Database = database,
            AppLogin = appLogin,
            Encrypt = builder.Encrypt,
            TrustServerCertificate = builder.TrustServerCertificate,
            EncryptedPassword = Protect(builder.Password)
        };
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(persisted, jsonOptions), Encoding.UTF8);
    }

    private static string Protect(string value)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Local encrypted database settings require Windows DPAPI.");
        var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(bytes);
    }

    private static string Unprotect(string value)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Local encrypted database settings require Windows DPAPI.");
        var bytes = ProtectedData.Unprotect(Convert.FromBase64String(value), null, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(bytes);
    }

    private sealed class PersistedDatabaseSettings
    {
        public string Provider { get; set; } = "SqlServer";
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string AppLogin { get; set; } = string.Empty;
        public bool Encrypt { get; set; } = true;
        public bool TrustServerCertificate { get; set; } = true;
        public string EncryptedPassword { get; set; } = string.Empty;
    }
}
