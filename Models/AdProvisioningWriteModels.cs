namespace Automind.CadastroColaboradores.Models;

public sealed class AdProvisioningCreateRequest : AdProvisioningValidationRequest
{
    public bool Confirmacao { get; set; }
}

public sealed class AdProvisioningWriteCommand
{
    public string Chamado { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Cn { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SamAccountName { get; set; } = string.Empty;
    public string UserPrincipalName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string? Office { get; set; }
    public string? TelephoneNumber { get; set; }
    public string? Title { get; set; }
    public string? Department { get; set; }
    public string Company { get; set; } = "Automind";
    public string ManagerDistinguishedName { get; set; } = string.Empty;
    public string OuDistinguishedName { get; set; } = string.Empty;
    public List<string> GroupDns { get; set; } = [];
}


public sealed class AdPilotMembershipRequest
{
    public bool Confirmacao { get; set; }
}

public sealed class AdPilotMembershipResponse
{
    public bool Success { get; set; }
    public bool Changed { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserDistinguishedName { get; set; }
    public string? GroupDistinguishedName { get; set; }
}

public sealed class AdProvisioningStepResult
{
    public string Key { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class AdProvisioningCreateResponse
{
    public bool Success { get; set; }
    public bool UserCreated { get; set; }
    public bool Enabled { get; set; }
    public bool RequiresManualReview { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DistinguishedName { get; set; }
    public string? TemporaryPassword { get; set; }
    public List<AdProvisioningStepResult> Steps { get; set; } = [];
}

public sealed class ProvisioningAuditEntry
{
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public string OperationId { get; set; } = string.Empty;
    public string Chamado { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string TechnicalIdentity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DistinguishedName { get; set; }
    public string? OuDistinguishedName { get; set; }
    public List<string> Attributes { get; set; } = [];
    public List<string> Groups { get; set; } = [];
    public string? ErrorType { get; set; }
    public int? ErrorCode { get; set; }
}
