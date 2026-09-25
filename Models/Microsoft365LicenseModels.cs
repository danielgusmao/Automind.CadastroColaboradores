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
}
