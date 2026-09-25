using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.Internal.Persistence;

internal sealed class InternalOrganizationConfiguration : IEntityTypeConfiguration<InternalOrganization>
{
    public void Configure(EntityTypeBuilder<InternalOrganization> builder)
    {
        builder.ToTable("InternalOrganization", table =>
            table.HasCheckConstraint("CK_InternalOrganization_Singleton", "`Id` = 1"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.InstallationId).IsUnique();
        builder.Property(x => x.LegalName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TradingName).HasMaxLength(300);
        builder.Property(x => x.LegalForm).HasMaxLength(100);
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.CompanyRegistrationNumber).HasMaxLength(100);
        builder.Property(x => x.DefaultCurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DefaultLanguageCode).HasMaxLength(10).IsRequired();
    }
}

internal sealed class ApplicationSettingConfiguration : IEntityTypeConfiguration<ApplicationSetting>
{
    public void Configure(EntityTypeBuilder<ApplicationSetting> builder)
    {
        builder.ToTable("ApplicationSettings");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasMaxLength(200);
        builder.Property(x => x.Value).HasColumnType("longtext").IsRequired();
        builder.HasIndex(x => x.UpdatedAtUtc);
    }
}
