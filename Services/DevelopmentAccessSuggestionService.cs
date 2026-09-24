using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

// Fallback de desenvolvimento. Nao e registrado no Program.cs na configuracao atual.
// Mantido para que workspaces antigos que ainda possuam o arquivo compilem sem reintroduzir grupos ficticios.
public sealed class DevelopmentAccessSuggestionService : IAccessSuggestionService
{
    public Task<IReadOnlyList<GroupSuggestion>> SuggestAsync(
        string? cargo,
        string? departamento,
        string? excludedSamAccountName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<GroupSuggestion>>([]);
    }

    public Task<IReadOnlyList<GroupSuggestion>> SearchGroupsAsync(
        string? query,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<GroupSuggestion>>([]);
    }

    public Task<IReadOnlyList<GroupSuggestion>> ResolveGroupsAsync(
        IEnumerable<string> distinguishedNames,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<GroupSuggestion>>([]);
    }
}
