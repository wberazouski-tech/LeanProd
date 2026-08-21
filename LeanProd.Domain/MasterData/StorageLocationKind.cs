namespace LeanProd.Domain.MasterData;

public sealed class StorageLocationKind
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<StorageLocation> StorageLocations { get; set; } = [];
}
