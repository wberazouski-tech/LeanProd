namespace LeanProd.Domain.MasterData;

public sealed class UnitOfMeasureTranslation
{
    public Guid UnitOfMeasureId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
