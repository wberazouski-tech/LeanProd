using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Features.MasterData;
using LeanProd.Api.Features.Organizations.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.Organizations;

[ApiController, Route("api/organization"), Authorize(Policy = Permissions.OrganizationView)]
public sealed class OrganizationController(IOrganizationService organizations, IAddressService addresses) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OrganizationDetails>> Get(CancellationToken ct)
    { var value = await organizations.GetAsync(ct); return value is null ? NotFound() : Ok(value); }

    [HttpPut, Authorize(Policy = Permissions.OrganizationManage)]
    public async Task<ActionResult<OrganizationDetails>> Save(SaveOrganizationRequest x, CancellationToken ct) =>
        Map(await organizations.SaveAsync(new(x.LegalName, x.TradingName, x.LegalForm, x.CountryCode,
            x.TaxNumber, x.StatisticalNumber, x.CompanyRegistrationNumber, x.DefaultCurrencyCode,
            x.TimeZoneId, x.DefaultLanguageCode, x.Email, x.Phone, x.Website, x.LogoFileId,
            x.PrintFooter, x.RowVersion), ct));

    [HttpGet("addresses")]
    public async Task<ActionResult<IReadOnlyCollection<AddressDetails>>> GetAddresses(CancellationToken ct) =>
        Ok(await addresses.GetAsync(new(AddressOwnerType.Organization), ct));

    [HttpPost("addresses"), Authorize(Policy = Permissions.OrganizationManage)]
    public async Task<ActionResult<AddressDetails>> CreateAddress(SaveAddressRequest x, CancellationToken ct) =>
        Map(await addresses.CreateAsync(new(AddressOwnerType.Organization), Command(x), ct));

    internal static SaveAddressCommand Command(SaveAddressRequest x) =>
        new(x.AddressType, x.CountryCode, x.Locality, x.PostalCode, x.AddressLine, x.Gln, x.IsPrimary, x.RowVersion);
}
