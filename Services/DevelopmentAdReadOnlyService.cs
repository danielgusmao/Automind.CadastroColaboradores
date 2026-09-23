using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

// Fallback de desenvolvimento. Nao e registrado no Program.cs na configuracao atual.
// Mantido apenas para compatibilidade com clones/workspaces que ainda possuam este arquivo.
// Todos os retornos falham de forma segura e nunca simulam sucesso de escrita/leitura real do AD.
public sealed class DevelopmentAdReadOnlyService : IAdReadOnlyService
{
    public Task<IReadOnlyList<OrganizationalUnitOption>> GetOrganizationalUnitsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<OrganizationalUnitOption>>([]);
    }

    public Task<AdIdentityAvailability> CheckIdentityAvailabilityAsync(
        string login,
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AdIdentityAvailability
        {
            LoginAvailable = false,
            UpnAvailable = false,
            EmailAvailable = false,
            Collisions = ["Servico de desenvolvimento sem consulta real ao Active Directory."]
        });
    }

    public Task<AdCommonNameAvailability> CheckCommonNameAvailabilityAsync(
        string commonName,
        string organizationalUnitDistinguishedName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AdCommonNameAvailability { Available = false });
    }

    public Task<AdUserResolution> ResolveUserAsync(string value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AdUserResolution());
    }

    public Task<AdOuValidation> ValidateOrganizationalUnitAsync(
        string distinguishedName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AdOuValidation { Exists = false, Allowed = false });
    }

    public Task<AdGroupValidation> ValidateGroupsAsync(
        IEnumerable<string> distinguishedNames,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requested = distinguishedNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult(new AdGroupValidation
        {
            AllExist = false,
            MissingGroups = requested
        });
    }
}
