using System;
using System.IO;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using LeanProd.Application.Common.Abstractions;

namespace LeanProd.Api.Common.Persistence;

/// <summary>Creates the DbContext for dotnet-ef without starting the web host.</summary>
public sealed class LeanProdDbContextFactory : IDesignTimeDbContextFactory<LeanProdDbContext>
{
    public LeanProdDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<LeanProdDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings:DefaultConnection in User Secrets or " +
                "ConnectionStrings__DefaultConnection in the environment.");

        var options = new DbContextOptionsBuilder<LeanProdDbContext>();
        if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlite(connectionString);
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(LeanProdDbContext).Assembly.FullName));
        }
        else
        {
            throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
        }

        return new LeanProdDbContext(options.Options, DesignTimeCurrentUser.Instance, TimeProvider.System);
    }

    private sealed class DesignTimeCurrentUser : ICurrentUser
    {
        public static DesignTimeCurrentUser Instance { get; } = new();
        public Guid? UserId => null;
    }
}
