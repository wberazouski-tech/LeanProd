namespace LeanProd.Domain.MasterData.Policies;

public static class StorageLocationPolicy
{
    public static string? ValidateInput(string? code, string? name, IReadOnlyCollection<Guid> typeIds)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return "Code and name are required.";
        if (code.Trim().Length != 4)
            return "Storage location code must contain exactly 4 characters.";
        return typeIds.Count == 0 ? "At least one storage location type is required." : null;
    }

    public static string? ValidateReferences(bool departmentIsActive, bool kindIsActive,
        int requestedTypeCount, int activeTypeCount)
    {
        if (!departmentIsActive) return "The department must be active.";
        if (!kindIsActive) return "The storage location kind is invalid.";
        return requestedTypeCount != activeTypeCount
            ? "One or more storage location types are invalid."
            : null;
    }
}
