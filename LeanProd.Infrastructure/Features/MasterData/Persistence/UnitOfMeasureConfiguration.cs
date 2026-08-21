using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitOfMeasures");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(4).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Symbol).HasMaxLength(30);
        builder.Property(x => x.LetterCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.QuantityType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.LetterCode).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}

internal sealed class UnitOfMeasureTranslationConfiguration : IEntityTypeConfiguration<UnitOfMeasureTranslation>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasureTranslation> builder)
    {
        builder.ToTable("UnitOfMeasureTranslations");
        builder.HasKey(x => new { x.UnitOfMeasureId, x.LanguageCode });
        builder.Property(x => x.LanguageCode).HasMaxLength(10);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.UnitOfMeasure).WithMany(x => x.Translations)
            .HasForeignKey(x => x.UnitOfMeasureId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class UnitOfMeasureConversionConfiguration : IEntityTypeConfiguration<UnitOfMeasureConversion>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasureConversion> builder)
    {
        builder.ToTable("UnitOfMeasureConversions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Multiplier).HasPrecision(28, 12);
        builder.Property(x => x.Offset).HasPrecision(28, 12);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.FromUnitId, x.ToUnitId }).IsUnique();
        builder.HasOne(x => x.FromUnit).WithMany(x => x.ConversionsFrom)
            .HasForeignKey(x => x.FromUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ToUnit).WithMany(x => x.ConversionsTo)
            .HasForeignKey(x => x.ToUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
