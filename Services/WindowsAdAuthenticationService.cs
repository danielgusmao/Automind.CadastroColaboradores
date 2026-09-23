using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;

namespace Automind.CadastroColaboradores.Services;

// Somente autenticação e consulta de grupo. Nenhuma escrita no AD.
[SupportedOSPlatform("windows")]
public sealed class WindowsAdAuthenticationService(
    IConfiguration configuration) : IAdAuthenticationService
{
    public Task<bool> AuthenticateAsync(
        string usuario,
        string senha,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var login = NormalizeLogin(usuario);

        if (string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrWhiteSpace(senha))
        {
            return Task.FromResult(false);
        }

        var configuredServer = configuration["Automind:Ad:Server"];
        var server = string.IsNullOrWhiteSpace(configuredServer)
            ? "10.1.2.1"
            : configuredServer.Trim();

        // Mesmo padrão de conexão utilizado no AutomindTermos.
        using var context = new PrincipalContext(
            ContextType.Domain,
            server);

        if (!context.ValidateCredentials(
                login,
                senha,
                ContextOptions.Negotiate))
        {
            return Task.FromResult(false);
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var user = UserPrincipal.FindByIdentity(
            context,
            IdentityType.SamAccountName,
            login);

        if (user is null || user.Enabled != true)
        {
            return Task.FromResult(false);
        }

        var configuredGroup = configuration["Automind:Ad:AuthorizedGroup"];
        var groupName = string.IsNullOrWhiteSpace(configuredGroup)
            ? "_informatica"
            : configuredGroup.Trim();

        using var group = GroupPrincipal.FindByIdentity(
            context,
            IdentityType.SamAccountName,
            groupName);

        if (group is null)
        {
            return Task.FromResult(false);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Mesmo método usado pelo Termos para consultar associação ao grupo.
        // Aqui a associação é obrigatória para entrar no Cadastro.
        return Task.FromResult(user.IsMemberOf(group));
    }

    private static string NormalizeLogin(string usuario)
    {
        var login = (usuario ?? string.Empty).Trim();

        if (login.Contains('\\'))
            login = login[(login.LastIndexOf('\\') + 1)..];

        if (login.Contains('@'))
            login = login[..login.IndexOf('@')];

        return login.Trim();
    }
}