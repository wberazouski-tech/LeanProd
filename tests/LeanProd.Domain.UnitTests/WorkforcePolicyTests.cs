using LeanProd.Domain.Workforce.Policies;
using Xunit;

namespace LeanProd.Domain.UnitTests;

public sealed class WorkforcePolicyTests
{
    private static readonly DateTime Start = new(2026, 8, 1, 6, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10.0001)]
    public void KtuOutsideAllowedRangeIsRejected(decimal coefficient) =>
        Assert.NotNull(BrigadeMembershipPolicy.Validate(Start, null, coefficient));

    [Theory]
    [InlineData(0.0001)]
    [InlineData(1)]
    [InlineData(2.5)]
    [InlineData(10)]
    public void ValidKtuIsAccepted(decimal coefficient) =>
        Assert.Null(BrigadeMembershipPolicy.Validate(Start, null, coefficient));

    [Fact]
    public void KtuWithMoreThanFourDecimalPlacesIsRejected() =>
        Assert.NotNull(BrigadeMembershipPolicy.Validate(Start, null, 1.00001m));

    [Fact]
    public void TouchingHalfOpenIntervalsDoNotOverlap()
    {
        var boundary = Start.AddHours(8);

        Assert.False(BrigadeMembershipPolicy.Overlaps(Start, boundary, boundary, null));
    }

    [Fact]
    public void IntersectingMembershipIntervalsOverlap() =>
        Assert.True(BrigadeMembershipPolicy.Overlaps(
            Start, Start.AddHours(8), Start.AddHours(7), null));
}
