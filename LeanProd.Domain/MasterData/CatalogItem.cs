using LeanProd.Domain.Common;

namespace LeanProd.Domain.MasterData;

public sealed class CatalogItem : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string WorkingName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? ArticleNumber { get; set; }
    public CatalogItemType Type { get; set; }
    public Guid CatalogItemClassId { get; set; }
    public CatalogItemClass CatalogItemClass { get; set; } = null!;
    public Guid BaseUnitOfMeasureId { get; set; }
    public UnitOfMeasure BaseUnitOfMeasure { get; set; } = null!;
    public ICollection<CatalogItemCostHistory> CostHistory { get; set; } = [];
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
