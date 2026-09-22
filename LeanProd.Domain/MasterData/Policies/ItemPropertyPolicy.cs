namespace LeanProd.Domain.MasterData.Policies;

public static class ItemPropertyPolicy
{
    public const decimal LargestNumber = 999999999999999999.999999m;

    public static string? ValidateDefinition(ItemPropertyDefinition p)
    {
        if (string.IsNullOrWhiteSpace(p.Code) || p.Code.Length > 50) return "Property code is required (up to 50 characters).";
        if (string.IsNullOrWhiteSpace(p.Name) || p.Name.Length > 200) return "Property name is required (up to 200 characters).";
        if (!Enum.IsDefined(p.Type)) return "Unknown property type.";
        var numeric = p.Type is ItemPropertyType.Number or ItemPropertyType.Range;
        if (numeric ? p.DecimalPlaces is null or < 0 or > 6 : p.DecimalPlaces is not null)
            return "Decimal places must be 0 to 6 for numbers and ranges only.";
        if (p.Type == ItemPropertyType.Text ? p.MaxLength is null or < 1 or > 4000 : p.MaxLength is not null)
            return "Maximum length must be 1 to 4000 for text only.";
        if (p.Type == ItemPropertyType.Range)
        {
            if (p.Minimum is null || p.Maximum is null || p.Minimum > p.Maximum)
                return "A range requires an ordered minimum and maximum.";
            if (!ValidNumber(p.Minimum.Value, p.DecimalPlaces!.Value) || !ValidNumber(p.Maximum.Value, p.DecimalPlaces.Value))
                return "Range limits exceed the configured precision or storage capacity.";
        }
        else if (p.Minimum is not null || p.Maximum is not null) return "Limits are supported for ranges only.";
        if (p.Options.Count > 500) return "At most 500 choices are supported.";
        if (p.Type == ItemPropertyType.Choice)
        {
            if (p.Options.Count == 0) return "At least one choice is required.";
            if (p.Options.Any(x => string.IsNullOrWhiteSpace(x.Label) || x.Label.Length > 200)) return "Choice labels must contain 1 to 200 characters.";
            if (p.Options.Select(x => x.Label.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != p.Options.Count)
                return "Choice labels must be unique.";
        }
        else if (p.Options.Count > 0) return "Choices are supported for choice properties only.";
        return null;
    }

    public static string? ValidateValue(ItemPropertyDefinition p, decimal? number, decimal? upper,
        string? text, bool? boolean, Guid? optionId)
    {
        // A completely empty value clears the property; zero and false are actual values.
        if (number is null && upper is null && text is null && boolean is null && optionId is null) return null;
        var validShape = p.Type switch
        {
            ItemPropertyType.Number => number is not null && upper is null && text is null && boolean is null && optionId is null,
            ItemPropertyType.Range => number is not null && upper is not null && text is null && boolean is null && optionId is null,
            ItemPropertyType.Text => text is not null && number is null && upper is null && boolean is null && optionId is null,
            ItemPropertyType.Boolean => boolean is not null && number is null && upper is null && text is null && optionId is null,
            ItemPropertyType.Choice => optionId is not null && number is null && upper is null && text is null && boolean is null,
            _ => false
        };
        if (!validShape) return "Value does not match the property type.";
        if (number.HasValue && !ValidNumber(number.Value, p.DecimalPlaces!.Value) ||
            upper.HasValue && !ValidNumber(upper.Value, p.DecimalPlaces!.Value))
            return "Number exceeds the configured decimal places or storage capacity.";
        if (p.Type == ItemPropertyType.Range && (number > upper || number < p.Minimum || upper > p.Maximum))
            return "Range must be ordered and within the configured limits.";
        if (text is not null && text.Length > p.MaxLength) return "Text exceeds the maximum length.";
        if (optionId.HasValue && !p.Options.Any(x => x.Id == optionId)) return "Choose a value from this property's list.";
        return null;
    }

    private static bool ValidNumber(decimal value, int scale) =>
        value >= -LargestNumber && value <= LargestNumber && decimal.Round(value, scale) == value;
}
