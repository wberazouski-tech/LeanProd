namespace LeanProd.Application.Features.MasterData;

public interface IEquipmentService
{
    Task<MasterDataPage<EquipmentSummary>> GetEquipmentAsync(EquipmentQuery query, CancellationToken ct);
    Task<EquipmentDetails?> GetEquipmentAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<EquipmentOption>> GetEquipmentOptionsAsync(CancellationToken ct);
    Task<MasterDataResult<EquipmentDetails>> CreateEquipmentAsync(SaveEquipmentCommand command, CancellationToken ct);
    Task<MasterDataResult<EquipmentDetails>> UpdateEquipmentAsync(Guid id, SaveEquipmentCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetEquipmentActiveAsync(Guid id, bool active, CancellationToken ct);
    Task<IReadOnlyCollection<EquipmentTypeDetails>> GetEquipmentTypesAsync(bool activeOnly, CancellationToken ct);
    Task<MasterDataResult<EquipmentTypeDetails>> CreateEquipmentTypeAsync(SaveEquipmentTypeCommand command, CancellationToken ct);
    Task<MasterDataResult<EquipmentTypeDetails>> UpdateEquipmentTypeAsync(Guid id, SaveEquipmentTypeCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetEquipmentTypeActiveAsync(Guid id, bool active, CancellationToken ct);
    Task<IReadOnlyCollection<EquipmentStateEventDetails>> GetStateHistoryAsync(Guid equipmentId, CancellationToken ct);
    Task<MasterDataResult<EquipmentStateEventDetails>> ChangeStateAsync(Guid equipmentId, ChangeEquipmentStateCommand command, CancellationToken ct);
    Task<MasterDataResult<EquipmentStateEventDetails>> UpdateStateAsync(Guid equipmentId, Guid eventId, ChangeEquipmentStateCommand command, CancellationToken ct);
}

public sealed record EquipmentQuery(int Page, int PageSize, string? Search, bool? IsActive,
    Guid? DepartmentId, Guid? EquipmentTypeId, string? State);
public sealed record EquipmentSummary(Guid Id, string Name, string? InventoryNumber, string? TypeName,
    string DepartmentName, string? ParentName, string? ParentInventoryNumber, string? CurrentState, bool IsActive);
public sealed record EquipmentDetails(Guid Id, string Name, string? InventoryNumber, Guid? EquipmentTypeId,
    Guid DepartmentId, Guid? ParentEquipmentId, string? SerialNumber, string? Manufacturer, string? Model,
    DateOnly? CommissionedOn, string? Description, string? CurrentState, bool IsActive, string RowVersion);
public sealed record EquipmentOption(Guid Id, string Name, string? InventoryNumber, Guid DepartmentId);
public sealed record SaveEquipmentCommand(string Name, string? InventoryNumber, Guid? EquipmentTypeId,
    Guid DepartmentId, Guid? ParentEquipmentId, string? SerialNumber, string? Manufacturer, string? Model,
    DateOnly? CommissionedOn, string? Description, string? RowVersion);
public sealed record EquipmentTypeDetails(Guid Id, string Name, string? Description, bool IsActive, string RowVersion);
public sealed record SaveEquipmentTypeCommand(string Name, string? Description, bool IsActive, string? RowVersion);
public sealed record EquipmentStateEventDetails(Guid Id, string State, DateTime StartedAtUtc,
    DateTime? EndedAtUtc, string? Comment, string RowVersion);
public sealed record ChangeEquipmentStateCommand(string State, DateTime StartedAtUtc, DateTime? EndedAtUtc,
    string? Comment, string? RowVersion);
