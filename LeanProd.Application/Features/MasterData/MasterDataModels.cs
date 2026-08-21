namespace LeanProd.Application.Features.MasterData;

public sealed record MasterDataQuery(int Page, int PageSize, string? Search, bool? IsActive);
public sealed record MasterDataPage<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);
public sealed record OptionItem(Guid Id, string Code, string Name);
public sealed record StorageLocationOption(Guid Id, string Code, string Name, Guid DepartmentId);
public sealed record CatalogItem(Guid Id, string Code);

public sealed record DepartmentSummary(Guid Id, string Code, string Name, Guid? ParentDepartmentId, string? ParentName, bool IsActive);
public sealed record DepartmentDetails(Guid Id, string Code, string Name, string? Description,
    Guid? ParentDepartmentId, bool IsActive, string RowVersion);
public sealed record SaveDepartmentCommand(string Code, string Name, string? Description,
    Guid? ParentDepartmentId, string? RowVersion);

public sealed record StorageLocationSummary(Guid Id, string Code, string Name, Guid? ParentStorageLocationId,
    string DepartmentName, string KindCode, IReadOnlyCollection<string> TypeCodes, bool IsActive);
public sealed record StorageLocationDetails(Guid Id, string Code, string Name,
    string? Description, Guid DepartmentId, Guid KindId, Guid? ParentStorageLocationId,
    IReadOnlyCollection<Guid> TypeIds, bool IsActive, string RowVersion);
public sealed record SaveStorageLocationCommand(string Code, string Name,
    string? Description, Guid DepartmentId, Guid KindId, Guid? ParentStorageLocationId,
    IReadOnlyCollection<Guid> TypeIds, string? RowVersion);

public enum MasterDataError { None, NotFound, Validation, Conflict, Dependency }
public sealed record MasterDataResult<T>(T? Value, MasterDataError Error, string? Message = null)
{
    public bool Succeeded => Error == MasterDataError.None;
    public static MasterDataResult<T> Success(T value) => new(value, MasterDataError.None);
    public static MasterDataResult<T> Failure(MasterDataError error, string message) => new(default, error, message);
}
