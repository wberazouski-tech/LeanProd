using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.ToTable("CatalogItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.WorkingName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(500);
        builder.Property(x => x.ArticleNumber).HasMaxLength(100);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => new { x.Type, x.ArticleNumber })
            .IsUnique()
            .HasFilter("[ArticleNumber] IS NOT NULL");
        builder.HasIndex(x => new { x.Type, x.IsActive, x.WorkingName });
        builder.HasOne(x => x.CatalogItemClass)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CatalogItemClassId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.BaseUnitOfMeasure)
            .WithMany()
            .HasForeignKey(x => x.BaseUnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
