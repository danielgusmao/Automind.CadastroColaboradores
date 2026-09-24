using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;

namespace Automind.CadastroColaboradores.Services;

// Autenticação e consulta de autorização. Nenhuma escrita no AD.
[SupportedOSPlatform("windows")]
public sealed class WindowsAdAuthenticationService(IConfiguration configuration) : IAdAuthenticationService
{
    public Task<bool> AuthenticateAsync(
        string usuario,
        string senha,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var login = NormalizeLogin(usuario);
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            return Task.FromResult(false);

        using var context = CreateContext();
        if (!context.ValidateCredentials(login, senha, ContextOptions.Negotiate))
            return Task.FromResult(false);

        return Task.FromResult(IsAuthorized(context, login, cancellationToken));
    }

    public Task<bool> IsAuthorizedAsync(string usuario, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var login = NormalizeLogin(usuario);
        if (string.IsNullOrWhiteSpace(login)) return Task.FromResult(false);

        using var context = CreateContext();
        return Task.FromResult(IsAuthorized(context, login, cancellationToken));
    }

    private PrincipalContext CreateContext()
    {
        var configuredServer = configuration["Automind:Ad:Server"];
        var server = string.IsNullOrWhiteSpace(configuredServer) ? "10.1.2.1" : configuredServer.Trim();
        return new PrincipalContext(ContextType.Domain, server);
    }

    private bool IsAuthorized(PrincipalContext context, string login, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, login);
        if (user is null || user.Enabled != true) return false;

        var configuredGroup = configuration["Automind:Ad:AuthorizedGroup"];
        var groupName = string.IsNullOrWhiteSpace(configuredGroup) ? "_informatica" : configuredGroup.Trim();

        using var group = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, groupName);
        if (group is null) return false;

        cancellationToken.ThrowIfCancellationRequested();
        return user.IsMemberOf(group);
    }

    private static string NormalizeLogin(string usuario)
    {
        var login = (usuario ?? string.Empty).Trim();
        if (login.Contains('\\')) login = login[(login.LastIndexOf('\\') + 1)..];
        if (login.Contains('@')) login = login[..login.IndexOf('@')];
        return login.Trim();
    }
}
