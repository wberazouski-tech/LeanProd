namespace LeanProd.Domain.MasterData.Policies;

public static class HierarchyPolicy
{
    public static string? Validate(Guid? entityId, Guid? parentId, IReadOnlyCollection<Guid> ancestorIds,
        string entityName)
    {
        if (entityId is null)
            return null;

        if (parentId == entityId)
            return $"{entityName} cannot be its own parent.";

        if (!ancestorIds.Contains(entityId.Value)) return null;
        var hierarchyName = entityName.StartsWith("An ", StringComparison.Ordinal)
            ? entityName[3..]
            : entityName.StartsWith("A ", StringComparison.Ordinal) ? entityName[2..] : entityName;
        return $"The {hierarchyName.ToLowerInvariant()} hierarchy cannot contain a cycle.";
    }
}
