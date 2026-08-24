using LeanProd.Application.Common.Abstractions;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Infrastructure.Features.MasterData;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LeanProd.Infrastructure.Features.Identity;
using LeanProd.Application.Features.Organizations;
using LeanProd.Infrastructure.Features.Organizations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LeanProd.Infrastructure.Common.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured. Use .NET User Secrets locally.");

        services.AddDbContext<LeanProdDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            if (!provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Unsupported database provider '{provider}'.");

            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(LeanProdDbContext).Assembly.FullName));
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LeanProdDbContext>());
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddSingleton<IInternationalUnitCatalog, InternationalUnitCatalog>();
        services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();
        services.AddScoped<ICatalogItemService, CatalogItemService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => options.SigningKey.Length >= 64, "JWT signing key must be at least 64 characters.")
            .ValidateOnStart();
        services.AddScoped<TokenService>();
        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<LeanProdDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        return services;
    }
}
