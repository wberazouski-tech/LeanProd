using LeanProd.Application.Common.Abstractions;
using LeanProd.Infrastructure.Common.Persistence;
using LeanProd.Infrastructure.Features.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InternalIdentityDbContext = LeanProd.Infrastructure.Common.Persistence.IdentityDbContext;

namespace LeanProd.Api.IntegrationTests;

public sealed class DatabaseSeparationContractTests
{
    [Fact]
    public void Business_model_contains_erp_tables_and_excludes_identity_tables()
    {
        var options = new DbContextOptionsBuilder<LeanProdDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=True;TrustServerCertificate=True")
            .Options;
        using var db = new LeanProdDbContext(options, NoCurrentUser.Instance, TimeProvider.System);
        var script = db.Database.GenerateCreateScript();

        Assert.Contains("ErpDatabaseInfo", script);
        Assert.Contains("CatalogItems", script);
        Assert.DoesNotContain("AspNetUsers", script);
        Assert.DoesNotContain("RefreshTokens", script);
    }

    [Fact]
    public void Internal_model_contains_identity_and_settings_but_excludes_erp_tables()
    {
        var options = new DbContextOptionsBuilder<InternalIdentityDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new InternalIdentityDbContext(options);
        var script = db.Database.GenerateCreateScript();

        Assert.Contains("AspNetUsers", script);
        Assert.Contains("RefreshTokens", script);
        Assert.Contains("InternalOrganization", script);
        Assert.Contains("ApplicationSettings", script);
        Assert.DoesNotContain("CatalogItems", script);
        Assert.DoesNotContain("Departments", script);

        var user = db.Model.FindEntityType(typeof(AppUser))!;
        Assert.DoesNotContain(user.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType.Namespace?.StartsWith("LeanProd.Domain", StringComparison.Ordinal) == true);
    }

    private sealed class NoCurrentUser : ICurrentUser
    {
        public static NoCurrentUser Instance { get; } = new();
        public Guid? UserId => null;
    }
}
