namespace LeanProd.Domain.MasterData;

public sealed class UnitOfMeasure
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public string LetterCode { get; set; } = string.Empty;
    public string QuantityType { get; set; } = string.Empty;
    public byte DecimalPlaces { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
    public List<UnitOfMeasureTranslation> Translations { get; set; } = [];
    public List<UnitOfMeasureConversion> ConversionsFrom { get; set; } = [];
    public List<UnitOfMeasureConversion> ConversionsTo { get; set; } = [];
}
