using LeanProd.Domain.Common;

namespace LeanProd.Domain.MasterData;

public sealed class CatalogItemCostHistory : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CatalogItemId { get; set; }
    public CatalogItem CatalogItem { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime EffectiveFromUtc { get; set; }
}
