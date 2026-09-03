using LeanProd.Domain.Workforce;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.Workforce.Persistence;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PersonnelNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MiddleName).HasMaxLength(100);
        builder.Property(x => x.Position).HasMaxLength(200);
        builder.HasIndex(x => x.PersonnelNumber).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.LastName, x.FirstName });
        builder.HasOne(x => x.Department).WithMany(x => x.Employees)
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class BrigadeConfiguration : IEntityTypeConfiguration<Brigade>
{
    public void Configure(EntityTypeBuilder<Brigade> builder)
    {
        builder.ToTable("Brigades");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.Name });
        builder.HasOne(x => x.Department).WithMany(x => x.Brigades)
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class BrigadeMembershipConfiguration : IEntityTypeConfiguration<BrigadeMembership>
{
    public void Configure(EntityTypeBuilder<BrigadeMembership> builder)
    {
        builder.ToTable("BrigadeMemberships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LaborParticipationCoefficient).HasPrecision(9, 4)
            .HasDefaultValue(1m).IsRequired();
        builder.HasIndex(x => new { x.BrigadeId, x.EmployeeId, x.StartedAtUtc }).IsUnique();
        builder.HasIndex(x => new { x.EmployeeId, x.StartedAtUtc, x.EndedAtUtc });
        builder.HasIndex(x => new { x.BrigadeId, x.StartedAtUtc, x.EndedAtUtc });
        builder.HasOne(x => x.Brigade).WithMany(x => x.Memberships)
            .HasForeignKey(x => x.BrigadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Employee).WithMany(x => x.BrigadeMemberships)
            .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
