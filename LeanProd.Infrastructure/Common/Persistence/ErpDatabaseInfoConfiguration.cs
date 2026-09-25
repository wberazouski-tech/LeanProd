using LeanProd.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Common.Persistence;

internal sealed class ErpDatabaseInfoConfiguration : IEntityTypeConfiguration<ErpDatabaseInfo>
{
    public void Configure(EntityTypeBuilder<ErpDatabaseInfo> builder)
    {
        builder.ToTable("ErpDatabaseInfo", table =>
            table.HasCheckConstraint("CK_ErpDatabaseInfo_Singleton", "[Id] = 1"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.DatabaseId).IsUnique();
        builder.Property(x => x.OrganizationLegalName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.OrganizationTaxNumber).HasMaxLength(50);
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(x => x.CreatedByApplicationVersion).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SchemaVersion).HasMaxLength(150).IsRequired();
    }
}
