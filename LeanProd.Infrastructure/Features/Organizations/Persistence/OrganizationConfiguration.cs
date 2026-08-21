using LeanProd.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.Organizations.Persistence;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organization", table => table.HasCheckConstraint("CK_Organization_Singleton", "[Id] = 1"));
        builder.HasKey(x => x.Id); builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.LegalName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TradingName).HasMaxLength(300); builder.Property(x => x.LegalForm).HasMaxLength(100);
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(x => x.TaxNumber).HasMaxLength(50); builder.Property(x => x.StatisticalNumber).HasMaxLength(50);
        builder.Property(x => x.CompanyRegistrationNumber).HasMaxLength(100);
        builder.Property(x => x.DefaultCurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DefaultLanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254); builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Website).HasMaxLength(300); builder.Property(x => x.PrintFooter).HasMaxLength(1000);
    }
}

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses", table => table.HasCheckConstraint("CK_Addresses_OneOwner",
            "(CASE WHEN [OrganizationId] IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN [DepartmentId] IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN [StorageLocationId] IS NOT NULL THEN 1 ELSE 0 END) = 1"));
        builder.HasKey(x => x.Id); builder.Property(x => x.AddressType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(x => x.Locality).HasMaxLength(200); builder.Property(x => x.PostalCode).HasMaxLength(30);
        builder.Property(x => x.AddressLine).HasMaxLength(500).IsRequired(); builder.Property(x => x.Gln).HasMaxLength(20);
        builder.HasIndex(x => x.Gln).IsUnique().HasFilter("[Gln] IS NOT NULL");
        builder.HasIndex(x => new { x.OrganizationId, x.IsActive, x.IsPrimary });
        builder.HasIndex(x => new { x.DepartmentId, x.IsActive, x.IsPrimary });
        builder.HasIndex(x => new { x.StorageLocationId, x.IsActive, x.IsPrimary });
        builder.HasOne(x => x.Organization).WithMany(x => x.Addresses).HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Department).WithMany(x => x.Addresses).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StorageLocation).WithMany(x => x.Addresses).HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}
