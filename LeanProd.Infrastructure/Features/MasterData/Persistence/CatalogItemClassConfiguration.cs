using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class CatalogItemClassConfiguration : IEntityTypeConfiguration<CatalogItemClass>
{
    public void Configure(EntityTypeBuilder<CatalogItemClass> builder)
    {
        builder.ToTable("CatalogItemClasses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.Type, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.Type, x.ParentId, x.IsActive, x.Name });
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
