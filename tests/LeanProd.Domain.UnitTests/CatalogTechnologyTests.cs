using LeanProd.Domain.Technologies;
using Xunit;

namespace LeanProd.Domain.UnitTests;

public sealed class CatalogTechnologyTests
{
    [Fact]
    public void New_technology_starts_in_development()
    {
        var technology = new CatalogTechnology();

        Assert.Equal(CatalogTechnologyStatus.InDevelopment, technology.Status);
    }

    [Fact]
    public void Status_contains_only_supported_values()
    {
        Assert.Equal(
            [CatalogTechnologyStatus.InDevelopment, CatalogTechnologyStatus.Active, CatalogTechnologyStatus.NotUsed],
            Enum.GetValues<CatalogTechnologyStatus>());
    }
}
