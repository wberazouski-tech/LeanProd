namespace LeanProd.Application.Features.MasterData;

public interface IDepartmentService
{
    Task<MasterDataPage<DepartmentSummary>> GetDepartmentsAsync(MasterDataQuery query, CancellationToken ct);
    Task<DepartmentDetails?> GetDepartmentAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<OptionItem>> GetDepartmentOptionsAsync(CancellationToken ct);
    Task<IReadOnlyCollection<OptionItem>> GetWorkScheduleOptionsAsync(CancellationToken ct);
    Task<MasterDataResult<DepartmentDetails>> CreateDepartmentAsync(
        SaveDepartmentCommand command, CancellationToken ct);
    Task<MasterDataResult<DepartmentDetails>> UpdateDepartmentAsync(
        Guid id, SaveDepartmentCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetDepartmentActiveAsync(Guid id, bool active, CancellationToken ct);
}
