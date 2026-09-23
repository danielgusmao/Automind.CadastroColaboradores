namespace Automind.CadastroColaboradores.Services;

public interface IAdAuthenticationService
{
    Task<bool> AuthenticateAsync(string usuario, string senha, CancellationToken cancellationToken = default);
}
