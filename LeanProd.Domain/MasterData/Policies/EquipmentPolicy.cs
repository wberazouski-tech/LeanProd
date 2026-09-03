namespace LeanProd.Domain.MasterData.Policies;

public static class EquipmentPolicy
{
    public static string? ValidateInput(string? name, string? inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required.";
        return name.Trim().Length > 200 || Clean(inventoryNumber)?.Length > 50
            ? "Name or inventory number is too long."
            : null;
    }

    public static string? ValidateReferences(bool departmentIsActive, bool equipmentTypeIsActive,
        bool parentEquipmentIsActive)
    {
        if (!departmentIsActive) return "The department must be active.";
        if (!equipmentTypeIsActive) return "The equipment type must be active.";
        return !parentEquipmentIsActive ? "The parent equipment must be active." : null;
    }

    public static string? ValidateState(string? state, DateTime startedAtUtc, DateTime? endedAtUtc)
    {
        if (state is null || !EquipmentStateCodes.All.Contains(state, StringComparer.OrdinalIgnoreCase))
            return "Select a valid equipment state.";
        if (startedAtUtc.Kind != DateTimeKind.Utc ||
            (endedAtUtc is not null && endedAtUtc.Value.Kind != DateTimeKind.Utc))
            return "State dates must include the UTC offset.";
        return endedAtUtc is not null && endedAtUtc <= startedAtUtc
            ? "State end date must be later than its start date."
            : null;
    }

    public static string? ValidateStateEnd(DateTime start, DateTime? requestedEnd, DateTime? nextStart)
    {
        if (requestedEnd is not null && requestedEnd <= start)
            return "State end date must be later than its start date.";
        return requestedEnd is not null && nextStart is not null && requestedEnd > nextStart
            ? "The state interval overlaps the next state."
            : null;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
