using LeanProd.Application.Features.MasterData;
using LeanProd.Application.Features.Organizations;
using LeanProd.Domain.Organizations;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.Organizations;

public sealed class AddressService(LeanProdDbContext db) : IAddressService
{
    public async Task<IReadOnlyCollection<AddressDetails>> GetAsync(AddressOwner owner, CancellationToken ct)
    {
        var items = await OwnerQuery(owner).AsNoTracking().OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.AddressType).ToArrayAsync(ct);
        return items.Select(x => new AddressDetails(x.Id, x.AddressType, x.CountryCode, x.Locality,
            x.PostalCode, x.AddressLine, x.Gln, x.IsPrimary, x.IsActive,
            Convert.ToBase64String(x.RowVersion))).ToArray();
    }

    public async Task<MasterDataResult<AddressDetails>> CreateAsync(AddressOwner owner, SaveAddressCommand c, CancellationToken ct)
    {
        var validation = await Validate(c, owner, ct); if (validation is not null) return validation;
        var item = New(owner, c); db.Addresses.Add(item);
        if (item.IsPrimary) await ClearPrimary(owner, null, ct);
        try { await db.SaveChangesAsync(ct); return Success(item); }
        catch (DbUpdateException) { return Conflict<AddressDetails>("An address with this GLN already exists."); }
    }

    public async Task<MasterDataResult<AddressDetails>> UpdateAsync(Guid id, SaveAddressCommand c, CancellationToken ct)
    {
        var item = await db.Addresses.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound<AddressDetails>();
        var owner = Owner(item); var validation = await Validate(c, owner, ct); if (validation is not null) return validation;
        if (!SetVersion(item, c.RowVersion)) return Validation<AddressDetails>("Row version is required.");
        item.AddressType = NormalizeType(c.AddressType); item.CountryCode = c.CountryCode.Trim().ToUpperInvariant();
        item.Locality = Clean(c.Locality); item.PostalCode = Clean(c.PostalCode); item.AddressLine = c.AddressLine.Trim();
        item.Gln = Clean(c.Gln); item.IsPrimary = c.IsPrimary;
        if (item.IsPrimary) await ClearPrimary(owner, item.Id, ct);
        try { await db.SaveChangesAsync(ct); return Success(item); }
        catch (DbUpdateConcurrencyException) { return Conflict<AddressDetails>("The address was changed by another request."); }
        catch (DbUpdateException) { return Conflict<AddressDetails>("An address with this GLN already exists."); }
    }

    public async Task<MasterDataResult<bool>> SetActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var item = await db.Addresses.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound<bool>();
        item.IsActive = active; if (!active) item.IsPrimary = false; await db.SaveChangesAsync(ct); return MasterDataResult<bool>.Success(true);
    }

    public async Task<MasterDataResult<bool>> MakePrimaryAsync(Guid id, CancellationToken ct)
    {
        var item = await db.Addresses.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound<bool>();
        if (!item.IsActive) return Validation<bool>("Only an active address can be primary.");
        await ClearPrimary(Owner(item), item.Id, ct); item.IsPrimary = true; await db.SaveChangesAsync(ct); return MasterDataResult<bool>.Success(true);
    }

    private IQueryable<Address> OwnerQuery(AddressOwner owner) => owner.Type switch
    {
        AddressOwnerType.Organization => db.Addresses.Where(x => x.OrganizationId == Organization.SingletonId),
        AddressOwnerType.Department => db.Addresses.Where(x => x.DepartmentId == owner.EntityId),
        AddressOwnerType.StorageLocation => db.Addresses.Where(x => x.StorageLocationId == owner.EntityId),
        _ => throw new ArgumentOutOfRangeException(nameof(owner))
    };
    private async Task<MasterDataResult<AddressDetails>?> Validate(SaveAddressCommand c, AddressOwner owner, CancellationToken ct)
    {
        if (!AddressTypeCodes.All.Contains(c.AddressType, StringComparer.OrdinalIgnoreCase)) return Validation<AddressDetails>("Select a valid address type.");
        if (c.CountryCode.Trim().Length != 2 || string.IsNullOrWhiteSpace(c.AddressLine)) return Validation<AddressDetails>("Country and address are required.");
        if (Clean(c.Gln) is { } gln && (!gln.All(char.IsDigit) || gln.Length != 13 || !ValidGln(gln))) return Validation<AddressDetails>("GLN must contain 13 digits with a valid check digit.");
        var exists = owner.Type switch { AddressOwnerType.Organization => await db.Organizations.AnyAsync(x => x.Id == Organization.SingletonId, ct), AddressOwnerType.Department => await db.Departments.AnyAsync(x => x.Id == owner.EntityId, ct), AddressOwnerType.StorageLocation => await db.StorageLocations.AnyAsync(x => x.Id == owner.EntityId, ct), _ => false };
        return exists ? null : NotFound<AddressDetails>();
    }
    private async Task ClearPrimary(AddressOwner owner, Guid? exceptId, CancellationToken ct)
    { foreach (var x in await OwnerQuery(owner).Where(x => x.IsPrimary && x.Id != exceptId).ToArrayAsync(ct)) x.IsPrimary = false; }
    private static Address New(AddressOwner owner, SaveAddressCommand c) => new()
    {
        OrganizationId = owner.Type == AddressOwnerType.Organization ? Organization.SingletonId : null,
        DepartmentId = owner.Type == AddressOwnerType.Department ? owner.EntityId : null,
        StorageLocationId = owner.Type == AddressOwnerType.StorageLocation ? owner.EntityId : null,
        AddressType = NormalizeType(c.AddressType), CountryCode = c.CountryCode.Trim().ToUpperInvariant(),
        Locality = Clean(c.Locality), PostalCode = Clean(c.PostalCode), AddressLine = c.AddressLine.Trim(),
        Gln = Clean(c.Gln), IsPrimary = c.IsPrimary
    };
    private static AddressOwner Owner(Address x) => x.OrganizationId is not null ? new(AddressOwnerType.Organization) : x.DepartmentId is not null ? new(AddressOwnerType.Department, x.DepartmentId) : new(AddressOwnerType.StorageLocation, x.StorageLocationId);
    private bool SetVersion(Address item, string? value) { try { if (string.IsNullOrWhiteSpace(value)) return false; db.Entry(item).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(value); return true; } catch (FormatException) { return false; } }
    private static bool ValidGln(string value) { var sum = 0; for (var i = 0; i < 12; i++) sum += (value[i] - '0') * (i % 2 == 0 ? 1 : 3); return (10 - sum % 10) % 10 == value[12] - '0'; }
    private static string NormalizeType(string value) => AddressTypeCodes.All.Single(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MasterDataResult<AddressDetails> Success(Address x) => MasterDataResult<AddressDetails>.Success(new(x.Id, x.AddressType, x.CountryCode, x.Locality, x.PostalCode, x.AddressLine, x.Gln, x.IsPrimary, x.IsActive, Convert.ToBase64String(x.RowVersion)));
    private static MasterDataResult<T> NotFound<T>() => MasterDataResult<T>.Failure(MasterDataError.NotFound, "Record was not found.");
    private static MasterDataResult<T> Validation<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<T> Conflict<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Conflict, message);
}
