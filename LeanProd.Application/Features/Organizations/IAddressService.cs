using LeanProd.Application.Features.MasterData;

namespace LeanProd.Application.Features.Organizations;

public enum AddressOwnerType { Organization, Department, StorageLocation }
public sealed record AddressOwner(AddressOwnerType Type, Guid? EntityId = null);

public interface IAddressService
{
    Task<IReadOnlyCollection<AddressDetails>> GetAsync(AddressOwner owner, CancellationToken ct);
    Task<MasterDataResult<AddressDetails>> CreateAsync(AddressOwner owner, SaveAddressCommand command, CancellationToken ct);
    Task<MasterDataResult<AddressDetails>> UpdateAsync(Guid id, SaveAddressCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetActiveAsync(Guid id, bool active, CancellationToken ct);
    Task<MasterDataResult<bool>> MakePrimaryAsync(Guid id, CancellationToken ct);
}

public sealed record AddressDetails(Guid Id, string AddressType, string CountryCode, string? Locality,
    string? PostalCode, string AddressLine, string? Gln, bool IsPrimary, bool IsActive, string RowVersion);
public sealed record SaveAddressCommand(string AddressType, string CountryCode, string? Locality,
    string? PostalCode, string AddressLine, string? Gln, bool IsPrimary, string? RowVersion);
