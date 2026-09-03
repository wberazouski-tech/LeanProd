namespace LeanProd.Domain.MasterData.Policies;

public static class DepartmentPolicy
{
    public static string? ValidateInput(string? code, string? name)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return "Code and name are required.";
        return code.Trim().Length != 4
            ? "Department code must contain exactly 4 characters."
            : null;
    }
}
