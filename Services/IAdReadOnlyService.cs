using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface IAdReadOnlyService
{
    Task<IReadOnlyList<OrganizationalUnitOption>> GetOrganizationalUnitsAsync(CancellationToken cancellationToken = default);
    Task<AdIdentityAvailability> CheckIdentityAvailabilityAsync(string login, string email, CancellationToken cancellationToken = default);
    Task<AdCommonNameAvailability> CheckCommonNameAvailabilityAsync(string commonName, string organizationalUnitDistinguishedName, CancellationToken cancellationToken = default);
    Task<AdUserResolution> ResolveUserAsync(string value, CancellationToken cancellationToken = default);
    Task<AdOuValidation> ValidateOrganizationalUnitAsync(string distinguishedName, CancellationToken cancellationToken = default);
    Task<AdGroupValidation> ValidateGroupsAsync(IEnumerable<string> distinguishedNames, CancellationToken cancellationToken = default);
}
