using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface IProvisioningAuditService
{
    Task AppendAsync(ProvisioningAuditEntry entry, CancellationToken cancellationToken = default);
}
