namespace LeanProd.Domain.MasterData.Policies;

public sealed record CatalogItemClassParentFacts(
    CatalogItemType Type,
    bool IsGroup,
    bool IsActive);

public static class CatalogItemClassPolicy
{
    public static string? ValidateInput(CatalogItemType type, string? code, string? name)
    {
        if (!Enum.IsDefined(type)) return "Item type is invalid.";
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return "Code and name are required.";
        if (code.Trim().Length > 50) return "Item class code cannot exceed 50 characters.";
        return name.Trim().Length > 200 ? "Item class name cannot exceed 200 characters." : null;
    }

    public static string? ValidateGroupFlagChange(bool currentIsGroup, bool requestedIsGroup,
        bool hasChildrenOrItems) =>
        currentIsGroup != requestedIsGroup && hasChildrenOrItems
            ? "Item class group flag cannot be changed while it has children or items."
            : null;

    public static string? ValidateParent(CatalogItemType type, CatalogItemClassParentFacts? parent)
    {
        if (parent is null || !parent.IsActive) return "The parent item class must be active.";
        if (!parent.IsGroup) return "The parent item class must be a group.";
        return parent.Type != type ? "The parent item class must have the same item type." : null;
    }
}
