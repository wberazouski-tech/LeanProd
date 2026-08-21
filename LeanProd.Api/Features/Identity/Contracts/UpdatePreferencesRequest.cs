using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.Identity.Contracts;

public sealed record UpdatePreferencesRequest([Required] string PreferredLanguage);
