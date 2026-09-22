using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface IAccessSuggestionService
{
    Task<IReadOnlyList<GroupSuggestion>> SuggestAsync(string? cargo, string? departamento, CancellationToken cancellationToken = default);
}
