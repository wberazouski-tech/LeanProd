using System.Reflection;
using System.Text.Json;
using LeanProd.Application.Features.MasterData;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class InternationalUnitCatalog : IInternationalUnitCatalog
{
    private readonly IReadOnlyDictionary<string, InternationalUnitCatalogItem> byCode;
    public IReadOnlyCollection<InternationalUnitCatalogItem> Items { get; }

    public InternationalUnitCatalog()
    {
        var assembly = typeof(InternationalUnitCatalog).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(x => x.EndsWith("international-units.catalog.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("International unit catalog resource was not found.");
        var document = JsonSerializer.Deserialize<CatalogDocument>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("International unit catalog is invalid.");
        Items = document.Units.Select(x => new InternationalUnitCatalogItem(x.Code, x.Name, x.Symbol, x.LetterCode)).ToArray();
        byCode = Items.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
    }

    public InternationalUnitCatalogItem? Find(string code) => byCode.GetValueOrDefault(code.Trim());

    private sealed record CatalogDocument(IReadOnlyCollection<CatalogItem> Units);
    private sealed record CatalogItem(string Code, string Name, string? Symbol, string LetterCode);
}
