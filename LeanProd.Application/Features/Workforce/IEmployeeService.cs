using LeanProd.Application.Features.MasterData;

namespace LeanProd.Application.Features.Workforce;

public interface IEmployeeService
{
    Task<MasterDataPage<EmployeeSummary>> GetEmployeesAsync(WorkforceQuery query, CancellationToken ct);
    Task<EmployeeDetails?> GetEmployeeAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<EmployeeOption>> GetEmployeeOptionsAsync(CancellationToken ct);
    Task<IReadOnlyCollection<BrigadeMembershipDetails>> GetBrigadeHistoryAsync(Guid id, CancellationToken ct);
    Task<WorkforceResult<EmployeeDetails>> CreateEmployeeAsync(SaveEmployeeCommand command, CancellationToken ct);
    Task<WorkforceResult<EmployeeDetails>> UpdateEmployeeAsync(Guid id, SaveEmployeeCommand command, CancellationToken ct);
    Task<WorkforceResult<bool>> SetEmployeeActiveAsync(Guid id, bool active, CancellationToken ct);
}
