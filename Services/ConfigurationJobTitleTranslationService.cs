using System.Globalization;
using System.Text;

namespace Automind.CadastroColaboradores.Services;

public sealed class ConfigurationJobTitleTranslationService : IJobTitleTranslationService
{
    private readonly Dictionary<string, string> _translations;

    public ConfigurationJobTitleTranslationService(IConfiguration configuration)
    {
        _translations = configuration
            .GetSection("Automind:JobTitleTranslations")
            .GetChildren()
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => NormalizeKey(x.Key), x => x.Value!.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public string? Translate(string? portugueseTitle)
    {
        var key = NormalizeKey(portugueseTitle);
        if (string.IsNullOrWhiteSpace(key)) return null;
        return _translations.TryGetValue(key, out var value) ? value : null;
    }

    private static string NormalizeKey(string? value)
    {
        var text = (value ?? string.Empty).Trim();
        if (text.Length == 0) return string.Empty;

        var decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var previousWasSpace = false;
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsWhiteSpace(character))
            {
                if (!previousWasSpace) builder.Append(' ');
                previousWasSpace = true;
                continue;
            }

            previousWasSpace = false;
            builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).Trim().ToUpperInvariant();
    }
}
