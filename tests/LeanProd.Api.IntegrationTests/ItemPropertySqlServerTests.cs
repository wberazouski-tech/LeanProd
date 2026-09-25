using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanProd.Api.IntegrationTests;

public sealed class ItemPropertySqlServerTests(LeanProdApiFactory factory) : IClassFixture<LeanProdApiFactory>
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Batch_snapshots_are_independent_and_stale_updates_are_atomic()
    {
        var (classId, itemId) = await Seed();
        var definition = (await Run(s => s.SaveDefinition(classId, null,
            new("Density", ItemPropertyType.Number, 2, null, null, null, true, true, [], null), Ct))).Value!;
        var initial = (await Run(s => s.Values(itemId, null, Ct))).Value!;
        var saved = await Run(s => s.SaveValues(itemId, null, new(initial.RowVersion, [new(definition.Id, 100m)]), Ct));
        Assert.True(saved.Succeeded, saved.Message);
        Assert.True(Assert.Single((await Run(s => s.Definitions(classId, Ct))).Value!).IsInUse);
        var batch = (await Run(s => s.SaveBatch(itemId, null, new("LOT-1", new DateOnly(2026, 9, 21), "Receipt 1", null), Ct))).Value!;
        var batchValues = (await Run(s => s.Values(itemId, batch.Id, Ct))).Value!;
        Assert.Equal(100m, Assert.Single(batchValues.Values).Number);

        var current = (await Run(s => s.Values(itemId, null, Ct))).Value!;
        Assert.True((await Run(s => s.SaveValues(itemId, null, new(current.RowVersion, [new(definition.Id, 110m)]), Ct))).Succeeded);
        Assert.Equal(100m, Assert.Single((await Run(s => s.Values(itemId, batch.Id, Ct))).Value!.Values).Number);
        Assert.True((await Run(s => s.SaveValues(itemId, batch.Id, new(batchValues.RowVersion, [new(definition.Id, 105m)]), Ct))).Succeeded);
        Assert.Equal(110m, Assert.Single((await Run(s => s.Values(itemId, null, Ct))).Value!.Values).Number);
        var stale = await Run(s => s.SaveValues(itemId, batch.Id, new(batchValues.RowVersion, [new(definition.Id, 999m)]), Ct));
        Assert.Equal(MasterDataError.Conflict, stale.Error);
        Assert.Equal(105m, Assert.Single((await Run(s => s.Values(itemId, batch.Id, Ct))).Value!.Values).Number);

        var duplicate = await Run(s => s.SaveBatch(itemId, null, new("lot-1", new DateOnly(2026, 9, 21), "Other", null), Ct));
        Assert.Equal(MasterDataError.Conflict, duplicate.Error);
        var changed = await Run(s => s.SaveDefinition(classId, definition.Id,
            new("Density", ItemPropertyType.Number, 1, null, null, null, true, true, [], definition.RowVersion), Ct));
        Assert.Equal(MasterDataError.Conflict, changed.Error);
    }

    [Fact]
    public async Task Foreign_properties_and_non_batch_values_are_rejected_without_partial_writes()
    {
        var (classId, itemId) = await Seed();
        var property = (await Run(s => s.SaveDefinition(classId, null,
            new("Flag", ItemPropertyType.Boolean, null, null, null, null, false, true, [], null), Ct))).Value!;
        var initial = (await Run(s => s.Values(itemId, null, Ct))).Value!;
        var rejected = await Run(s => s.SaveValues(itemId, null,
            new(initial.RowVersion, [new(property.Id, Boolean: false), new(Guid.NewGuid(), Number: 1)]), Ct));
        Assert.Equal(MasterDataError.Validation, rejected.Error);
        Assert.Empty((await Run(s => s.Values(itemId, null, Ct))).Value!.Values);
        Assert.False(Assert.Single((await Run(s => s.Definitions(classId, Ct))).Value!).IsInUse);
        var batch = (await Run(s => s.SaveBatch(itemId, null, new("B", new DateOnly(2026, 9, 21), "Receipt", null), Ct))).Value!;
        rejected = await Run(s => s.SaveValues(itemId, batch.Id, new(batch.RowVersion, [new(property.Id, Boolean: false)]), Ct));
        Assert.Equal(MasterDataError.Validation, rejected.Error);
        Assert.Empty((await Run(s => s.Values(itemId, batch.Id, Ct))).Value!.Values);
        var (_, otherItemId) = await Seed();
        Assert.Equal(MasterDataError.NotFound, (await Run(s => s.Values(otherItemId, batch.Id, Ct))).Error);
    }

    [Fact]
    public async Task Class_change_with_property_values_is_rejected_through_existing_service()
    {
        var (classId, itemId) = await Seed();
        var (otherClassId, _) = await Seed();
        var property = (await Run(s => s.SaveDefinition(classId, null,
            new("Flag", ItemPropertyType.Boolean, null, null, null, null, false, true, [], null), Ct))).Value!;
        var initial = (await Run(s => s.Values(itemId, null, Ct))).Value!;
        Assert.True((await Run(s => s.SaveValues(itemId, null, new(initial.RowVersion, [new(property.Id, Boolean: false)]), Ct))).Succeeded);
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICatalogItemService>();
        Assert.Equal(MasterDataError.Conflict, (await service.ChangeItemClassAsync(itemId, otherClassId, Ct)).Error);
    }

    [Fact]
    public async Task Http_routes_enforce_authentication_and_bind_item_and_batch_paths()
    {
        var (classId, itemId) = await Seed();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/api/catalog-items/{itemId}/properties")).StatusCode);
        var login = await client.PostAsJsonAsync("/api/identity/login", new { email = LeanProdApiFactory.AdminEmail, password = LeanProdApiFactory.AdminPassword });
        login.EnsureSuccessStatusCode();
        var identity = await login.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.GetProperty("accessToken").GetString());
        var definition = await client.PostAsJsonAsync($"/api/catalog-item-classes/{classId}/properties", new
        {
            name = "HTTP flag", type = "Boolean", isBatchProperty = true, isActive = true, options = Array.Empty<object>()
        });
        definition.EnsureSuccessStatusCode();
        var propertyId = (await definition.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var itemValues = (await client.GetFromJsonAsync<PropertyValuesDto>($"/api/catalog-items/{itemId}/properties", Json))!;
        var save = await client.PutAsJsonAsync($"/api/catalog-items/{itemId}/properties", new SavePropertyValuesCommand(itemValues.RowVersion, [new(propertyId, Boolean: false)]));
        save.EnsureSuccessStatusCode();
        var batchResponse = await client.PostAsJsonAsync($"/api/catalog-items/{itemId}/batches", new SaveBatchCommand("HTTP", new DateOnly(2026, 9, 21), "Receipt", null));
        batchResponse.EnsureSuccessStatusCode();
        var batch = (await batchResponse.Content.ReadFromJsonAsync<BatchDto>())!;
        var batchValues = (await client.GetFromJsonAsync<PropertyValuesDto>($"/api/catalog-items/{itemId}/batches/{batch.Id}/properties", Json))!;
        Assert.False(Assert.Single(batchValues.Values).Boolean);
        Assert.Equal(batch.Id, batchValues.Batch!.Id);
        var updated = await client.PutAsJsonAsync($"/api/catalog-items/{itemId}/batches/{batch.Id}/properties",
            new SavePropertyValuesCommand(batchValues.RowVersion, [new(propertyId, Boolean: true)]));
        updated.EnsureSuccessStatusCode();
        Assert.True(Assert.Single((await updated.Content.ReadFromJsonAsync<PropertyValuesDto>(Json))!.Values).Boolean);
    }

    private async Task<T> Run<T>(Func<IItemPropertyService, Task<T>> action)
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<IItemPropertyService>());
    }

    private async Task<(Guid ClassId, Guid ItemId)> Seed()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LeanProdDbContext>();
        var itemClass = new CatalogItemClass { Type = CatalogItemType.Product, Code = Guid.NewGuid().ToString("N"), Name = "Property test" };
        var unit = await db.UnitOfMeasures.FirstOrDefaultAsync();
        if (unit is null) { unit = new UnitOfMeasure { Code = "PROP", Name = "Unit", LetterCode = "p", QuantityType = "Count" }; db.UnitOfMeasures.Add(unit); }
        var item = new CatalogItem { WorkingName = "Property test", Type = CatalogItemType.Product, CatalogItemClass = itemClass, BaseUnitOfMeasure = unit };
        db.CatalogItems.Add(item);
        await db.SaveChangesAsync();
        return (itemClass.Id, item.Id);
    }
}
