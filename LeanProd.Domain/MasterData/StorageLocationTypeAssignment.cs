namespace LeanProd.Domain.MasterData;

public sealed class StorageLocationTypeAssignment
{
    public Guid StorageLocationId { get; set; }
    public StorageLocation StorageLocation { get; set; } = null!;
    public Guid StorageLocationTypeId { get; set; }
    public StorageLocationType StorageLocationType { get; set; } = null!;
}
