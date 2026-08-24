using LeanProd.Domain.Common;

namespace LeanProd.Domain.MasterData;

public sealed class CatalogItemClass : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public CatalogItemType Type { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public Guid? ParentId { get; set; }
    public CatalogItemClass? Parent { get; set; }
    public ICollection<CatalogItemClass> Children { get; set; } = [];
    public ICollection<CatalogItem> Items { get; set; } = [];
    public bool IsActive { get; set; } = true;
}
