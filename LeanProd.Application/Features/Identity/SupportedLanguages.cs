namespace LeanProd.Application.Features.Identity;

public static class SupportedLanguages
{
    public const string Belarusian = "be";
    public const string English = "en";
    public static readonly IReadOnlyCollection<string> All = [Belarusian, English];

    public static bool IsSupported(string? language) =>
        language is not null && All.Contains(language, StringComparer.OrdinalIgnoreCase);
}
