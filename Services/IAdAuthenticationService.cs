namespace Automind.CadastroColaboradores.Services;

public interface IAdAuthenticationService
{
    Task<bool> AuthenticateAsync(string usuario, string senha, CancellationToken cancellationToken = default);
    Task<bool> IsAuthorizedAsync(string usuario, CancellationToken cancellationToken = default);
}
