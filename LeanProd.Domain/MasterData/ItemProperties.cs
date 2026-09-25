using LeanProd.Domain.Common;

namespace LeanProd.Domain.MasterData;

public enum ItemPropertyType { Number, Text, Choice, Boolean, Range }

public sealed class ItemPropertyDefinition : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CatalogItemClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemPropertyType Type { get; set; }
    public int? DecimalPlaces { get; set; }
    public int? MaxLength { get; set; }
    public decimal? Minimum { get; set; }
    public decimal? Maximum { get; set; }
    public bool IsBatchProperty { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ItemPropertyOption> Options { get; set; } = [];
}

public sealed class ItemPropertyOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PropertyId { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class CatalogItemPropertyValue : AuditableEntity
{
    public Guid CatalogItemId { get; set; }
    public Guid PropertyId { get; set; }
    public decimal? Number { get; set; }
    public decimal? Upper { get; set; }
    public string? Text { get; set; }
    public bool? Boolean { get; set; }
    public Guid? OptionId { get; set; }
}

public sealed class CatalogItemBatch : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CatalogItemId { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateOnly ReceiptDate { get; set; }
    public string ReceiptReference { get; set; } = string.Empty;
    public ICollection<BatchPropertyValue> Values { get; set; } = [];
}

public sealed class BatchPropertyValue : AuditableEntity
{
    public Guid BatchId { get; set; }
    public Guid PropertyId { get; set; }
    public decimal? Number { get; set; }
    public decimal? Upper { get; set; }
    public string? Text { get; set; }
    public bool? Boolean { get; set; }
    public Guid? OptionId { get; set; }
}
