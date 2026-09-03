using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.Setup;

public sealed record DatabaseSetupStatus(
    bool IsConfigured,
    bool CanConfigureAnonymously,
    string? Server,
    string? Database,
    string? AppLogin);

public sealed record DatabaseAdminConnectionRequest(
    [Required, MaxLength(256)] string Server,
    [Required, MaxLength(128)] string Database,
    [Required, MaxLength(128)] string AdminLogin,
    [Required] string AdminPassword,
    bool Encrypt = true,
    bool TrustServerCertificate = true,
    [MaxLength(128)] string AppLogin = "leanprod_app");

public sealed record ConnectExistingDatabaseRequest(
    [Required] DatabaseAdminConnectionRequest Connection,
    bool ApplyMigrations = false);

public sealed record CreateDatabaseRequest(
    [Required] DatabaseAdminConnectionRequest Connection);

public sealed record DatabaseSetupResult(
    string Status,
    string Message,
    string? Server,
    string? Database,
    string? AppLogin,
    IReadOnlyList<string> AppliedMigrations,
    IReadOnlyList<string> PendingMigrations);

public static class DatabaseSetupStatuses
{
    public const string Connected = "Connected";
    public const string Created = "Created";
    public const string RequiresMigration = "RequiresMigration";
    public const string NotLeanProd = "NotLeanProd";
}

public sealed class DatabaseStartupState
{
    public bool DatabaseInitializationFailed { get; set; }
}
