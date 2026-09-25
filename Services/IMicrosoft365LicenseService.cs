using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface IMicrosoft365LicenseService
{
    bool IsEnabled { get; }
    bool LicenseWritesEnabled { get; }
    Task<IReadOnlyList<Microsoft365LicenseInfo>> GetSubscribedLicensesAsync(CancellationToken cancellationToken = default);
}
