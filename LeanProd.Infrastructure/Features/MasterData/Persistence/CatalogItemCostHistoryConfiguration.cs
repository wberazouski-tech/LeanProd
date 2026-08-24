using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class CatalogItemCostHistoryConfiguration : IEntityTypeConfiguration<CatalogItemCostHistory>
{
    public void Configure(EntityTypeBuilder<CatalogItemCostHistory> builder)
    {
        builder.ToTable("CatalogItemCostHistory",
            table => table.HasCheckConstraint("CK_CatalogItemCostHistory_Amount_NonNegative", "[Amount] >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.EffectiveFromUtc).IsRequired();
        builder.HasIndex(x => new { x.CatalogItemId, x.EffectiveFromUtc });
        builder.HasOne(x => x.CatalogItem)
            .WithMany(x => x.CostHistory)
            .HasForeignKey(x => x.CatalogItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
