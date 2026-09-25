using LeanProd.Domain.MasterData;
using LeanProd.Domain.MasterData.Policies;
using Xunit;

namespace LeanProd.Domain.UnitTests;

public sealed class ItemPropertyPolicyTests
{
    [Theory]
    [InlineData("12.34", 2, true)]
    [InlineData("12.345", 2, false)]
    [InlineData("12.3400", 2, true)]
    [InlineData("-12", 0, true)]
    [InlineData("0.000001", 6, true)]
    [InlineData("0.0000001", 6, false)]
    [InlineData("999999999999999999.999999", 6, true)]
    [InlineData("1000000000000000000", 0, false)]
    public void Numbers_preserve_precision_without_rounding(string input, int scale, bool valid)
    {
        var p = Definition(ItemPropertyType.Number); p.DecimalPlaces = scale;
        var error = ItemPropertyPolicy.ValidateValue(p, decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture), null, null, null, null);
        Assert.Equal(valid, error is null);
    }

    [Theory]
    [InlineData(100, 15000, true)]
    [InlineData(0, 0, true)]
    [InlineData(150, 100, false)]
    [InlineData(-1, 100, false)]
    [InlineData(100, 15001, false)]
    public void Range_enforces_inclusive_ordered_bounds(int lower, int upper, bool valid)
    {
        var p = Definition(ItemPropertyType.Range); p.DecimalPlaces = 2; p.Minimum = 0; p.Maximum = 15000;
        Assert.Equal(valid, ItemPropertyPolicy.ValidateValue(p, lower, upper, null, null, null) is null);
        Assert.NotNull(ItemPropertyPolicy.ValidateValue(p, lower, null, null, null, null));
    }

    [Fact]
    public void False_and_zero_are_values_and_mixed_payloads_are_rejected()
    {
        var p = Definition(ItemPropertyType.Boolean);
        Assert.Null(ItemPropertyPolicy.ValidateValue(p, null, null, null, false, null));
        Assert.Null(ItemPropertyPolicy.ValidateValue(p, null, null, null, null, null));
        Assert.NotNull(ItemPropertyPolicy.ValidateValue(p, 0, null, null, false, null));
        p.Type = ItemPropertyType.Number; p.DecimalPlaces = 0;
        Assert.Null(ItemPropertyPolicy.ValidateValue(p, 0, null, null, null, null));
    }

    [Fact]
    public void Text_length_and_choice_ownership_are_enforced()
    {
        var p = Definition(ItemPropertyType.Text); p.MaxLength = 3;
        Assert.Null(ItemPropertyPolicy.ValidateValue(p, null, null, "абв", null, null));
        Assert.NotNull(ItemPropertyPolicy.ValidateValue(p, null, null, "абвг", null, null));
        p = Definition(ItemPropertyType.Choice);
        var option = new ItemPropertyOption { Label = "A" }; p.Options.Add(option);
        Assert.Null(ItemPropertyPolicy.ValidateValue(p, null, null, null, null, option.Id));
        Assert.NotNull(ItemPropertyPolicy.ValidateValue(p, null, null, null, null, Guid.NewGuid()));
    }

    [Fact]
    public void Definitions_reject_incompatible_settings_and_duplicate_choices()
    {
        var p = Definition(ItemPropertyType.Number); p.DecimalPlaces = 7;
        Assert.NotNull(ItemPropertyPolicy.ValidateDefinition(p));
        p.DecimalPlaces = 2; Assert.Null(ItemPropertyPolicy.ValidateDefinition(p));
        p.MaxLength = 10; Assert.NotNull(ItemPropertyPolicy.ValidateDefinition(p));
        p = Definition(ItemPropertyType.Range); p.DecimalPlaces = 2; p.Minimum = 1; p.Maximum = 0;
        Assert.NotNull(ItemPropertyPolicy.ValidateDefinition(p));
        p.Minimum = 0; p.Maximum = 1.001m; Assert.NotNull(ItemPropertyPolicy.ValidateDefinition(p));
        p = Definition(ItemPropertyType.Choice);
        Assert.NotNull(ItemPropertyPolicy.ValidateDefinition(p));
        p.Options.Add(new() { Label = "A" }); p.Options.Add(new() { Label = "a" });
        Assert.NotNull(ItemPropertyPolicy.ValidateDefinition(p));
    }

    private static ItemPropertyDefinition Definition(ItemPropertyType type) => new() { Name = "Test", Type = type };
}
