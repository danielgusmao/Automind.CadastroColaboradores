using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface IAdProvisioningWriteService
{
    bool IsWriteModeEnabled { get; }
    bool GroupWritesEnabled { get; }
    IReadOnlySet<string> WriteAllowedOuDns { get; }
    string Mode { get; }
    Task<AdProvisioningCreateResponse> CreateUserAsync(AdProvisioningWriteCommand command, CancellationToken cancellationToken = default);
}
