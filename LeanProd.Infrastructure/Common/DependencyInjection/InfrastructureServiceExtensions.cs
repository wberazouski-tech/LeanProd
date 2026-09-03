using LeanProd.Application.Common.Abstractions;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Infrastructure.Features.MasterData;
using LeanProd.Application.Features.Technologies;
using LeanProd.Infrastructure.Features.Technologies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LeanProd.Infrastructure.Features.Identity;
using LeanProd.Application.Features.Workforce;
using LeanProd.Infrastructure.Features.Workforce;
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
        services.AddDbContext<LeanProdDbContext>((serviceProvider, options) =>
        {
            var database = serviceProvider.GetRequiredService<IRuntimeDatabaseConnection>();
            if (!database.IsConfigured)
                throw new InvalidOperationException(
                    "LeanProd database is not configured. Open database settings to connect or create a database.");

            if (database.Provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(database.ConnectionString);
                return;
            }

            if (!database.Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Unsupported database provider '{database.Provider}'.");

            options.UseSqlServer(database.ConnectionString, sql =>
                sql.MigrationsAssembly(typeof(LeanProdDbContext).Assembly.FullName));
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LeanProdDbContext>());
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();
        services.AddScoped<ICatalogItemClassService, CatalogItemClassService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IStorageLocationService, StorageLocationService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IBrigadeService, BrigadeService>();
        services.AddSingleton<IInternationalUnitCatalog, InternationalUnitCatalog>();
        services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();
        services.AddScoped<ICatalogItemService, CatalogItemService>();
        services.AddScoped<ICatalogTechnologyService, CatalogTechnologyService>();
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
