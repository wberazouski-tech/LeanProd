using System.Globalization;
using System.Text.Json;
using LeanProd.Api.Features.MasterData.Contracts;
using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using Xunit;

namespace LeanProd.Api.IntegrationTests;

public sealed class CatalogCostJsonTests
{
    [Theory]
    [InlineData("9999999999999999.99")]
    [InlineData("99999999999999.99")]
    [InlineData("0.01")]
    public void Cost_is_exact_in_response_and_request(string amount)
    {
        var cost = decimal.Parse(amount, CultureInfo.InvariantCulture);
        var item = new CatalogItemSummary(Guid.NewGuid(), "Item", null, null, CatalogItemType.Product,
            Guid.NewGuid(), "Unit", null, Guid.NewGuid(), "C", "Class", cost, true);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(item, options);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.String, document.RootElement.GetProperty("cost").ValueKind);
        Assert.Equal(amount, document.RootElement.GetProperty("cost").GetString());
        var request = JsonSerializer.Deserialize<SaveCatalogItemRequest>(json, options);
        Assert.Equal(cost, request!.Cost);
    }
}
