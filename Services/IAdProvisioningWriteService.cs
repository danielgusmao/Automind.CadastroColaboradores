using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface IAdProvisioningWriteService
{
    bool IsWriteModeEnabled { get; }
    bool GroupWritesEnabled { get; }
    bool PilotMembershipTestEnabled { get; }
    string PilotMembershipUserDn { get; }
    string PilotMembershipGroupDn { get; }
    IReadOnlySet<string> WriteAllowedOuDns { get; }
    IReadOnlySet<string> GroupWriteAllowedDns { get; }
    string Mode { get; }
    Task<AdProvisioningCreateResponse> CreateUserAsync(AdProvisioningWriteCommand command, CancellationToken cancellationToken = default);
    Task<AdPilotMembershipResponse> ApplyPilotMembershipAsync(string chamado, string operatorName, CancellationToken cancellationToken = default);
}
