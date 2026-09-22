namespace Automind.CadastroColaboradores.Models;

public sealed class TopdeskFormData
{
    public Dictionary<string, string> Campos { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public string? Get(string nome) => Campos.TryGetValue(nome, out var valor) ? valor : null;
}
