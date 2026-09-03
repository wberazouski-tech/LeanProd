using System.ComponentModel.DataAnnotations;

using System;

namespace LeanProd.Api.Features.Workforce.Contracts;

public sealed record SaveEmployeeRequest(
    [Required, MaxLength(50)] string PersonnelNumber,
    [Required, MaxLength(100)] string LastName,
    [Required, MaxLength(100)] string FirstName,
    [MaxLength(100)] string? MiddleName,
    [MaxLength(200)] string? Position,
    Guid? DepartmentId,
    string? RowVersion);

public sealed record SaveBrigadeRequest(
    [MaxLength(50)] string Code,
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    Guid? DepartmentId,
    string? RowVersion);

public sealed record AddBrigadeMembershipRequest(Guid EmployeeId, DateTime StartedAtUtc,
    DateTime? EndedAtUtc, decimal LaborParticipationCoefficient);
public sealed record CloseBrigadeMembershipRequest(DateTime EndedAtUtc, [Required] string RowVersion);
public sealed record ChangeMembershipCoefficientRequest(DateTime EffectiveFromUtc,
    decimal LaborParticipationCoefficient, [Required] string RowVersion);
