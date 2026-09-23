using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Versioning;

namespace Automind.CadastroColaboradores.Services;

// Somente autenticação e consulta do grupo de acesso. Nenhuma operação de escrita.
[SupportedOSPlatform("windows")]
public sealed class WindowsAdAuthenticationService(IConfiguration configuration) : IAdAuthenticationService
{
    private string GetDomainName()
    {
        var configured = configuration["Automind:Ad:Domain"];
        if (!string.IsNullOrWhiteSpace(configured)) return configured.Trim();
        using var domain = Domain.GetComputerDomain();
        return domain.Name;
    }

    public Task<bool> AuthenticateAsync(string usuario, string senha, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha)) return Task.FromResult(false);
        var login = usuario.Trim();
        // ValidateCredentials espera sAMAccountName, e não domínio\usuário ou UPN.
        if (login.IndexOfAny(['\\', '@']) >= 0) return Task.FromResult(false);
        using var context = new PrincipalContext(ContextType.Domain, GetDomainName(), null,
            ContextOptions.Negotiate | ContextOptions.Signing | ContextOptions.Sealing);
        if (!context.ValidateCredentials(login, senha, ContextOptions.Negotiate | ContextOptions.Signing | ContextOptions.Sealing))
            return Task.FromResult(false);

        cancellationToken.ThrowIfCancellationRequested();
        // As consultas usam a identidade do pool/servidor; senha do operador não é persistida.
        using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, login);
        if (user is null || user.Enabled != true || user.IsAccountLockedOut()) return Task.FromResult(false);
        if (user.AccountExpirationDate is DateTime expires && expires.ToUniversalTime() <= DateTime.UtcNow)
            return Task.FromResult(false);

        var configuredGroup = configuration["Automind:Ad:AuthorizedGroup"];
        var groupName = string.IsNullOrWhiteSpace(configuredGroup) ? "_informatica" : configuredGroup.Trim();
        using var allowed = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, groupName);
        if (allowed?.Sid is null) return Task.FromResult(false);
        using var groups = user.GetAuthorizationGroups();
        foreach (var group in groups)
        {
            using (group)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (allowed.Sid.Equals(group.Sid)) return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }
}
