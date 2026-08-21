using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(4).IsFixedLength().IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Code).IsUnique(); builder.HasIndex(x => x.IsActive);
        builder.HasOne(x => x.ParentDepartment).WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentDepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
