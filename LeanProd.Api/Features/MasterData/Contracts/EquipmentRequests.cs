using System;
using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.MasterData.Contracts;

public sealed record SaveEquipmentRequest(
    [Required, StringLength(200)] string Name,
    [StringLength(50)] string? InventoryNumber,
    Guid? EquipmentTypeId,
    Guid DepartmentId,
    Guid? ParentEquipmentId,
    [StringLength(100)] string? SerialNumber,
    [StringLength(200)] string? Manufacturer,
    [StringLength(100)] string? Model,
    DateOnly? CommissionedOn,
    [StringLength(1000)] string? Description,
    string? RowVersion);

public sealed record SaveEquipmentTypeRequest(
    [Required, StringLength(200)] string Name,
    [StringLength(1000)] string? Description,
    bool? IsActive,
    string? RowVersion);

public sealed record ChangeEquipmentStateRequest(
    [Required, StringLength(30)] string State,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    [StringLength(1000)] string? Comment,
    string? RowVersion);
