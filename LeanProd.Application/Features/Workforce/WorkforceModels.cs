using LeanProd.Application.Features.MasterData;

namespace LeanProd.Application.Features.Workforce;

public sealed record WorkforceQuery(int Page, int PageSize, string? Search, bool? IsActive, Guid? DepartmentId);

public sealed record EmployeeSummary(Guid Id, string PersonnelNumber, string FullName, string? Position,
    Guid? DepartmentId, string? DepartmentName, int ActiveBrigadeCount, bool IsActive);
public sealed record EmployeeDetails(Guid Id, string PersonnelNumber, string LastName, string FirstName,
    string? MiddleName, string? Position, Guid? DepartmentId, bool IsActive, string RowVersion);
public sealed record EmployeeOption(Guid Id, string PersonnelNumber, string FullName);
public sealed record SaveEmployeeCommand(string PersonnelNumber, string LastName, string FirstName,
    string? MiddleName, string? Position, Guid? DepartmentId, string? RowVersion);

public sealed record BrigadeSummary(Guid Id, string Code, string Name, Guid? DepartmentId,
    string? DepartmentName, int ActiveMemberCount, bool IsActive);
public sealed record BrigadeDetails(Guid Id, string Code, string Name, string? Description,
    Guid? DepartmentId, bool IsActive, string RowVersion);
public sealed record BrigadeOption(Guid Id, string Code, string Name);
public sealed record SaveBrigadeCommand(string Code, string Name, string? Description,
    Guid? DepartmentId, string? RowVersion);

public sealed record BrigadeMembershipDetails(Guid Id, Guid BrigadeId, string BrigadeCode,
    string BrigadeName, Guid EmployeeId, string PersonnelNumber, string EmployeeName,
    DateTime StartedAtUtc, DateTime? EndedAtUtc, decimal LaborParticipationCoefficient,
    string RowVersion);
public sealed record AddBrigadeMembershipCommand(Guid EmployeeId, DateTime StartedAtUtc,
    DateTime? EndedAtUtc, decimal LaborParticipationCoefficient);
public sealed record CloseBrigadeMembershipCommand(DateTime EndedAtUtc, string RowVersion);
public sealed record ChangeMembershipCoefficientCommand(DateTime EffectiveFromUtc,
    decimal LaborParticipationCoefficient, string RowVersion);

public enum WorkforceError { None, NotFound, Validation, Conflict, Dependency }
public sealed record WorkforceResult<T>(T? Value, WorkforceError Error, string? Message = null)
{
    public bool Succeeded => Error == WorkforceError.None;
    public static WorkforceResult<T> Success(T value) => new(value, WorkforceError.None);
    public static WorkforceResult<T> Failure(WorkforceError error, string message) =>
        new(default, error, message);
}
