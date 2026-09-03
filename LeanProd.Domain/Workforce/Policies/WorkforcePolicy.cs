namespace LeanProd.Domain.Workforce.Policies;

public static class WorkforcePolicy
{
    public static string? ValidateEmployee(string? personnelNumber, string? lastName, string? firstName)
    {
        if (string.IsNullOrWhiteSpace(personnelNumber) || string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(firstName))
            return "Personnel number, last name and first name are required.";
        if (personnelNumber.Trim().Length > 50) return "Personnel number cannot exceed 50 characters.";
        if (lastName.Trim().Length > 100 || firstName.Trim().Length > 100)
            return "Employee name cannot exceed 100 characters.";
        return null;
    }

    public static string? ValidateBrigade(string? code, string? name)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return "Code and name are required.";
        if (code.Trim().Length > 50) return "Brigade code cannot exceed 50 characters.";
        return name.Trim().Length > 200 ? "Brigade name cannot exceed 200 characters." : null;
    }
}
