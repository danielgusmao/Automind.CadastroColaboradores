using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface IMicrosoft365LicenseService
{
    bool IsEnabled { get; }
    bool LicenseWritesEnabled { get; }
    int SyncPollSeconds { get; }
    int SyncMaxWaitSeconds { get; }
    Task<IReadOnlyList<Microsoft365LicenseInfo>> GetSubscribedLicensesAsync(CancellationToken cancellationToken = default);
    Task<Microsoft365LicenseSelectionValidation> ValidateSelectionAsync(IEnumerable<Guid> skuIds, CancellationToken cancellationToken = default);
    Task<Microsoft365LicenseAssignmentResponse> AssignLicensesAsync(
        string chamado,
        string operatorName,
        string userPrincipalName,
        IEnumerable<Guid> skuIds,
        CancellationToken cancellationToken = default);
}
