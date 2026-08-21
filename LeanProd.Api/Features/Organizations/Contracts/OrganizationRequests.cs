using System;

namespace LeanProd.Api.Features.Organizations.Contracts;

public sealed record SaveOrganizationRequest(string LegalName, string? TradingName, string? LegalForm,
    string CountryCode, string? TaxNumber, string? StatisticalNumber, string? CompanyRegistrationNumber,
    string DefaultCurrencyCode, string TimeZoneId, string DefaultLanguageCode, string? Email, string? Phone,
    string? Website, Guid? LogoFileId, string? PrintFooter, string? RowVersion);

public sealed record SaveAddressRequest(string AddressType, string CountryCode, string? Locality,
    string? PostalCode, string AddressLine, string? Gln, bool IsPrimary, string? RowVersion);
