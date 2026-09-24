using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface IAccessSuggestionService
{
    Task<IReadOnlyList<GroupSuggestion>> SuggestAsync(
        string? cargo,
        string? departamento,
        string? excludedSamAccountName,
        string? excludedCommonName,
        string? excludedOuDistinguishedName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroupSuggestion>> SearchGroupsAsync(
        string? query,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroupSuggestion>> ResolveGroupsAsync(
        IEnumerable<string> distinguishedNames,
        CancellationToken cancellationToken = default);
}
