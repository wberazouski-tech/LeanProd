using LeanProd.Application.Common.Abstractions;
using LeanProd.Domain.Common;
using LeanProd.Infrastructure.Features.Identity;
using LeanProd.Domain.MasterData;
using LeanProd.Domain.Organizations;
using LeanProd.Domain.Technologies;
using LeanProd.Domain.Workforce;
using LeanProd.Domain.Scheduling;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Common.Persistence;

public sealed class LeanProdDbContext(
    DbContextOptions<LeanProdDbContext> options,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SecurityAuditEvent> SecurityAuditEvents => Set<SecurityAuditEvent>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<StorageLocationKind> StorageLocationKinds => Set<StorageLocationKind>();
    public DbSet<StorageLocationType> StorageLocationTypes => Set<StorageLocationType>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<UnitOfMeasureTranslation> UnitOfMeasureTranslations => Set<UnitOfMeasureTranslation>();
    public DbSet<UnitOfMeasureConversion> UnitOfMeasureConversions => Set<UnitOfMeasureConversion>();
    public DbSet<CatalogItemClass> CatalogItemClasses => Set<CatalogItemClass>();
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    public DbSet<ItemPropertyDefinition> ItemPropertyDefinitions => Set<ItemPropertyDefinition>();
    public DbSet<ItemPropertyOption> ItemPropertyOptions => Set<ItemPropertyOption>();
    public DbSet<CatalogItemPropertyValue> CatalogItemPropertyValues => Set<CatalogItemPropertyValue>();
    public DbSet<CatalogItemBatch> CatalogItemBatches => Set<CatalogItemBatch>();
    public DbSet<BatchPropertyValue> BatchPropertyValues => Set<BatchPropertyValue>();
    public DbSet<CatalogItemCostHistory> CatalogItemCostHistory => Set<CatalogItemCostHistory>();
    public DbSet<CatalogTechnology> CatalogTechnologies => Set<CatalogTechnology>();
    public DbSet<TechnologyStageTemplate> TechnologyStageTemplates => Set<TechnologyStageTemplate>();
    public DbSet<TechnologyStage> TechnologyStages => Set<TechnologyStage>();
    public DbSet<CatalogTechnologyStage> CatalogTechnologyStages => Set<CatalogTechnologyStage>();
    public DbSet<CatalogTechnologyStageTransition> CatalogTechnologyStageTransitions => Set<CatalogTechnologyStageTransition>();
    public DbSet<CatalogTechnologyMaterial> CatalogTechnologyMaterials => Set<CatalogTechnologyMaterial>();
    public DbSet<CatalogTechnologyStageOutput> CatalogTechnologyStageOutputs => Set<CatalogTechnologyStageOutput>();
    public DbSet<CatalogTechnologyOperation> CatalogTechnologyOperations => Set<CatalogTechnologyOperation>();
    public DbSet<CatalogTechnologyMaterialSupplyRouteStep> CatalogTechnologyMaterialSupplyRouteSteps => Set<CatalogTechnologyMaterialSupplyRouteStep>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<EquipmentType> EquipmentTypes => Set<EquipmentType>();
    public DbSet<EquipmentStateEvent> EquipmentStateEvents => Set<EquipmentStateEvent>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Brigade> Brigades => Set<Brigade>();
    public DbSet<BrigadeMembership> BrigadeMemberships => Set<BrigadeMembership>();
    public DbSet<WorkSchedule> WorkSchedules => Set<WorkSchedule>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<WorkScheduleDay> WorkScheduleDays => Set<WorkScheduleDay>();
    public DbSet<WorkScheduleInterval> WorkScheduleIntervals => Set<WorkScheduleInterval>();
    public DbSet<ProductionCalendar> ProductionCalendars => Set<ProductionCalendar>();
    public DbSet<ProductionCalendarDay> ProductionCalendarDays => Set<ProductionCalendarDay>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(LeanProdDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes()
                     .Where(type => typeof(AuditableEntity).IsAssignableFrom(type.ClrType)))
        {
            builder.Entity(entityType.ClrType)
                .Property(nameof(AuditableEntity.RowVersion))
                .IsRowVersion();
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditValues();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditValues();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditValues()
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var userId = currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedByUserId = userId;
                entry.Entity.UpdatedAtUtc = null;
                entry.Entity.UpdatedByUserId = null;
                continue;
            }

            if (entry.State != EntityState.Modified) continue;

            entry.Property(entity => entity.CreatedAtUtc).IsModified = false;
            entry.Property(entity => entity.CreatedByUserId).IsModified = false;
            entry.Entity.UpdatedAtUtc = now;
            entry.Entity.UpdatedByUserId = userId;
        }
    }
}
