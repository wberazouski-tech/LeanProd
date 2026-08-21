namespace LeanProd.Domain.MasterData;

public sealed class UnitOfMeasureConversion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FromUnitId { get; set; }
    public UnitOfMeasure FromUnit { get; set; } = null!;
    public Guid ToUnitId { get; set; }
    public UnitOfMeasure ToUnit { get; set; } = null!;
    public decimal Multiplier { get; set; }
    public decimal Offset { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
