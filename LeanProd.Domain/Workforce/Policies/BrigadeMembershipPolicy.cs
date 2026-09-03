namespace LeanProd.Domain.Workforce.Policies;

public static class BrigadeMembershipPolicy
{
    public const decimal MaximumCoefficient = 10m;

    public static string? Validate(DateTime startedAtUtc, DateTime? endedAtUtc, decimal coefficient)
    {
        if (startedAtUtc.Kind != DateTimeKind.Utc ||
            (endedAtUtc is not null && endedAtUtc.Value.Kind != DateTimeKind.Utc))
            return "Membership dates must include the UTC offset.";
        if (endedAtUtc is not null && endedAtUtc <= startedAtUtc)
            return "Membership end date must be later than its start date.";
        if (coefficient <= 0 || coefficient > MaximumCoefficient)
            return $"KTU must be greater than 0 and no greater than {MaximumCoefficient}.";
        return decimal.Round(coefficient, 4) != coefficient
            ? "KTU cannot have more than 4 decimal places."
            : null;
    }

    public static bool Overlaps(DateTime firstStart, DateTime? firstEnd,
        DateTime secondStart, DateTime? secondEnd) =>
        firstStart < (secondEnd ?? DateTime.MaxValue) &&
        secondStart < (firstEnd ?? DateTime.MaxValue);
}
