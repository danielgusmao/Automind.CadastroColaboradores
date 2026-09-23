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
            .ToDictionary(x => x.Key.Trim(), x => x.Value!.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public string? Translate(string? portugueseTitle)
    {
        var key = (portugueseTitle ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(key)) return null;
        return _translations.TryGetValue(key, out var value) ? value : null;
    }
}
