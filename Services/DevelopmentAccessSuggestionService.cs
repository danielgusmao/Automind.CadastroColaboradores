using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

// MOCK: depois sera substituido por leitura real do AD.
public sealed class DevelopmentAccessSuggestionService : IAccessSuggestionService
{
    public Task<IReadOnlyList<GroupSuggestion>> SuggestAsync(string? cargo, string? departamento, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GroupSuggestion> result = [
            new() { Nome = "GG_Colaboradores", EncontradoEm = 4, TotalComparados = 4, Selecionado = true },
            new() { Nome = "GG_Engenharia", EncontradoEm = 4, TotalComparados = 4, Selecionado = true },
            new() { Nome = "VPN_Usuarios", EncontradoEm = 4, TotalComparados = 4, Selecionado = true },
            new() { Nome = "Projeto_Temporario", EncontradoEm = 1, TotalComparados = 4, Selecionado = false },
            new() { Nome = "Domain Admins", EncontradoEm = 1, TotalComparados = 4, Selecionado = false, Protegido = true }
        ];
        return Task.FromResult(result);
    }
}
