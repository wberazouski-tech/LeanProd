using System;
using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.MasterData.Contracts;

public sealed record CreateUnitOfMeasureRequest(
    [Required, StringLength(4)] string CatalogCode,
    [Required, StringLength(50)] string QuantityType,
    [Range(0, 6)] byte DecimalPlaces,
    [Required, StringLength(10)] string LanguageCode,
    [StringLength(200)] string? LocalizedName);

public sealed record UpdateUnitOfMeasureRequest(
    [Required, StringLength(50)] string QuantityType,
    [Range(0, 6)] byte DecimalPlaces,
    [Required, StringLength(10)] string LanguageCode,
    [StringLength(200)] string? LocalizedName,
    [Required] string? RowVersion);

public sealed record CreateUnitConversionRequest(
    Guid FromUnitId,
    Guid ToUnitId,
    decimal Multiplier,
    decimal Offset);

public sealed record ConvertUnitRequest(Guid FromUnitId, Guid ToUnitId, decimal Value);
