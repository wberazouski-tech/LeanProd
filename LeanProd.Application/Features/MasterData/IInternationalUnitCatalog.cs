namespace LeanProd.Application.Features.MasterData;

public interface IInternationalUnitCatalog
{
    IReadOnlyCollection<InternationalUnitCatalogItem> Items { get; }
    InternationalUnitCatalogItem? Find(string code);
}

public sealed record InternationalUnitCatalogItem(string Code, string Name, string? Symbol, string LetterCode);
