using System.Text.RegularExpressions;
using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public sealed class TopdeskRequestParser : ITopdeskRequestParser
{
    private static readonly Regex Cabecalho = new(@"^\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}\s+.+?:\s*$", RegexOptions.Compiled);

    public TopdeskFormData Parse(string request)
    {
        var linhas = request
            .Replace("\r", string.Empty)
            .Split('\n')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (linhas.Count > 0 && Cabecalho.IsMatch(linhas[0]))
            linhas.RemoveAt(0);

        var campos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < linhas.Count - 1; i++)
        {
            if (linhas[i].StartsWith('-')) continue;
            if (!linhas[i + 1].StartsWith('-')) continue;

            campos[linhas[i]] = linhas[i + 1].TrimStart('-').Trim();
            i++;
        }

        // Observação pode conter múltiplas linhas após o primeiro valor.
        var obsIndex = linhas.FindIndex(x => x.Equals("Observação", StringComparison.OrdinalIgnoreCase));
        if (obsIndex >= 0 && obsIndex + 1 < linhas.Count && linhas[obsIndex + 1].StartsWith('-'))
        {
            campos["Observação"] = string.Join("\n", linhas.Skip(obsIndex + 1))
                .TrimStart('-').Trim();
        }

        return new TopdeskFormData { Campos = campos };
    }
}
