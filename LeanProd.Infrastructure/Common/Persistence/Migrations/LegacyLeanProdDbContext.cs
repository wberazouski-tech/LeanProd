using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Common.Persistence.Migrations;

/// <summary>
/// Identifies the pre-split migration history. It is retained for existing installations only;
/// new installations use BusinessInitialCreate and InternalInitialCreate.
/// </summary>
internal sealed class LegacyLeanProdDbContext(DbContextOptions<LegacyLeanProdDbContext> options)
    : DbContext(options);
