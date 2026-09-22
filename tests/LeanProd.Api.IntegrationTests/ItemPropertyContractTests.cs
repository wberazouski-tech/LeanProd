using System.Reflection;
using LeanProd.Api.Features.MasterData;
using LeanProd.Application.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using LeanProd.Application.Features.MasterData;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace LeanProd.Api.IntegrationTests;

public sealed class ItemPropertyContractTests
{
    [Fact]
    public void Every_property_write_requires_manage_permission()
    {
        var controller = typeof(ItemPropertiesController);
        Assert.Contains(controller.GetCustomAttributes<AuthorizeAttribute>(), x => x.Policy == Permissions.MasterDataView);
        foreach (var action in new[] { "CreateDefinition", "UpdateDefinition", "SaveValues", "CreateBatch", "UpdateBatch" })
            Assert.Contains(controller.GetMethod(action)!.GetCustomAttributes<AuthorizeAttribute>(), x => x.Policy == Permissions.MasterDataManage);
    }

    [Fact]
    public void Decimal_values_round_trip_as_strings_without_javascript_precision_loss()
    {
        var dto = new PropertyValueDto(Guid.NewGuid(), 999999999999999999.999999m, 999999999999999999.999999m);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(dto, options);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.String, document.RootElement.GetProperty("number").ValueKind);
        Assert.Equal("999999999999999999.999999", document.RootElement.GetProperty("number").GetString());
        Assert.Equal(dto, JsonSerializer.Deserialize<PropertyValueDto>(json, options));
    }

    [Fact]
    public void SqlServer_model_contains_restrictive_foreign_keys_and_decimal_columns()
    {
        using var db = new LeanProdDbContext(new DbContextOptionsBuilder<LeanProdDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options, null!, TimeProvider.System);
        var model = db.GetService<IDesignTimeModel>().Model;
        var entities = model.GetEntityTypes().Where(x => new[] { "ItemPropertyDefinitions", "ItemPropertyOptions", "CatalogItemPropertyValues", "CatalogItemBatches", "BatchPropertyValues" }.Contains(x.GetTableName())).ToArray();
        Assert.Equal(5, entities.Length);
        Assert.All(entities.SelectMany(x => x.GetForeignKeys()), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        Assert.All(entities.SelectMany(x => x.GetProperties()).Where(x => x.ClrType == typeof(decimal?)), p =>
        { Assert.Equal(24, p.GetPrecision()); Assert.Equal(6, p.GetScale()); });
        var sql = db.Database.GenerateCreateScript();
        Assert.Contains("CK_BatchPropertyValues_Shape", sql);
        Assert.Contains("CK_ItemPropertyDefinitions_Settings", sql);
    }
}
