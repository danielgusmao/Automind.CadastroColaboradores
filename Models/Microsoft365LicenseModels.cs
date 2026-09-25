namespace Automind.CadastroColaboradores.Models;

public sealed class Microsoft365LicenseInfo
{
    public Guid SkuId { get; init; }
    public string SkuPartNumber { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int EnabledUnits { get; init; }
    public int ConsumedUnits { get; init; }
    public int SuspendedUnits { get; init; }
    public string CapabilityStatus { get; init; } = string.Empty;

    public int AvailableUnits => Math.Max(0, EnabledUnits - ConsumedUnits);
    public bool IsUnlimited => EnabledUnits >= 1_000_000;
    public bool IsSuspended => string.Equals(CapabilityStatus, "Suspended", StringComparison.OrdinalIgnoreCase);
    public bool IsEnabled => string.Equals(CapabilityStatus, "Enabled", StringComparison.OrdinalIgnoreCase);
    public bool CanAssign => SkuId != Guid.Empty && IsEnabled && (IsUnlimited || AvailableUnits > 0);
}

public sealed class Microsoft365LicenseSelectionValidation
{
    public bool Valid { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<Microsoft365LicenseInfo> SelectedLicenses { get; init; } = [];
}

public sealed class Microsoft365LicenseAssignmentRequest
{
    public string? Chamado { get; set; }
    public string? UserPrincipalName { get; set; }
    public List<Guid> SelectedLicenseSkuIds { get; set; } = [];
    public bool Confirmacao { get; set; }
}

public sealed class Microsoft365LicenseAssignmentStepResult
{
    public string Key { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class Microsoft365LicenseAssignmentResponse
{
    public bool Success { get; init; }
    public bool PendingSynchronization { get; init; }
    public bool Changed { get; init; }
    public bool RequiresManualReview { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? UserPrincipalName { get; init; }
    public string? UsageLocation { get; init; }
    public int RetryAfterSeconds { get; init; }
    public List<Guid> AddedSkuIds { get; init; } = [];
    public List<string> AssignedLicenseNames { get; init; } = [];
    public List<Microsoft365LicenseAssignmentStepResult> Steps { get; init; } = [];
}
