namespace Automind.CadastroColaboradores.Services;

public interface IAdReadOnlyService
{
    Task<bool> ValidateCredentialsAsync(string usuario, string senha, CancellationToken cancellationToken = default);
    Task<bool> IsAuthorizedAsync(string usuario, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetOrganizationalUnitsAsync(CancellationToken cancellationToken = default);
}
