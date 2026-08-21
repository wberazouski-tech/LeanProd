using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.MasterData.Contracts;

public sealed record SaveDepartmentRequest(
    [Required, StringLength(4, MinimumLength = 4)] string Code,
    [Required, StringLength(200)] string Name,
    [StringLength(1000)] string? Description,
    Guid? ParentDepartmentId,
    string? RowVersion);

public sealed record SaveStorageLocationRequest(
    [Required, StringLength(4, MinimumLength = 4)] string Code,
    [Required, StringLength(200)] string Name,
    [StringLength(1000)] string? Description,
    Guid DepartmentId,
    Guid KindId,
    Guid? ParentStorageLocationId,
    [Required, MinLength(1)] IReadOnlyCollection<Guid> TypeIds,
    string? RowVersion);
