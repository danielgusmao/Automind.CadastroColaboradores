namespace Automind.CadastroColaboradores.Models;

public sealed class ProvisioningHistoryViewModel
{
    public List<ProvisioningHistoryItem> Items { get; set; } = [];
    public IReadOnlyList<Microsoft365LicenseInfo> AssignableLicenses { get; set; } = [];
}

public sealed class ProvisioningHistoryItem
{
    public string Chamado { get; set; } = string.Empty;
    public DateTimeOffset LastEventUtc { get; set; }
    public string Operator { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? UserPrincipalName { get; set; }
    public string? DistinguishedName { get; set; }
    public bool AdCompleted { get; set; }
    public bool AdFailed { get; set; }
    public List<string> Groups { get; set; } = [];
    public bool EntraExists { get; set; }
    public string? UsageLocation { get; set; }
    public bool Microsoft365Checked { get; set; }
    public string? Microsoft365Error { get; set; }
    public List<Microsoft365AssignedLicenseInfo> AssignedLicenses { get; set; } = [];
    public List<Guid> PendingLicenseSkuIds { get; set; } = [];
    public List<string> PendingLicenseNames { get; set; } = [];

    public bool HasAssignedLicenses => AssignedLicenses.Count > 0;
    public bool CanRecoverMicrosoft365 => AdCompleted && !AdFailed && !string.IsNullOrWhiteSpace(UserPrincipalName);
}

public sealed class Microsoft365UserLicenseStatus
{
    public string UserPrincipalName { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string? UsageLocation { get; init; }
    public bool? OnPremisesSyncEnabled { get; init; }
    public string? ErrorMessage { get; init; }
    public List<Microsoft365AssignedLicenseInfo> AssignedLicenses { get; init; } = [];
}

public sealed class Microsoft365AssignedLicenseInfo
{
    public Guid SkuId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string SkuPartNumber { get; init; } = string.Empty;
    public string? State { get; init; }
    public string? Error { get; init; }
    public bool IsHealthy => (string.IsNullOrWhiteSpace(Error) || string.Equals(Error, "None", StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrWhiteSpace(State) || string.Equals(State, "Active", StringComparison.OrdinalIgnoreCase));
}
