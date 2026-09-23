namespace Automind.CadastroColaboradores.Models;

public sealed class OrganizationalUnitOption
{
    public string DisplayName { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
}

public sealed class AdIdentityAvailability
{
    public bool LoginAvailable { get; set; }
    public bool UpnAvailable { get; set; }
    public bool EmailAvailable { get; set; }
    public List<string> Collisions { get; set; } = [];
}


public sealed class AdCommonNameAvailability
{
    public bool Available { get; set; }
    public string? ExistingDistinguishedName { get; set; }
}

public sealed class AdUserResolution
{
    public bool Found { get; set; }
    public bool Ambiguous { get; set; }
    public string? DisplayName { get; set; }
    public string? SamAccountName { get; set; }
    public string? DistinguishedName { get; set; }
    public int Matches { get; set; }
}

public sealed class AdOuValidation
{
    public bool Exists { get; set; }
    public bool Allowed { get; set; }
    public string? DisplayName { get; set; }
    public string? DistinguishedName { get; set; }
}

public sealed class AdGroupValidation
{
    public bool AllExist { get; set; }
    public List<string> MissingGroups { get; set; } = [];
}

public sealed class AdProvisioningValidationRequest
{
    public string? NomeCompleto { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? TelefoneCelular { get; set; }
    public bool DivulgarContato { get; set; }
    public string? LocalTrabalho { get; set; }
    public string? SuperiorImediato { get; set; }
    public string? CargoIngles { get; set; }
    public string? Departamento { get; set; }
    public string? PerfilUsuario { get; set; }
    public string? OuDistinguishedName { get; set; }
    public List<string> SelectedGroupDns { get; set; } = [];
}

public sealed class AdValidationCheck
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class AdProvisioningPreview
{
    public string Cn { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string SamAccountName { get; set; } = string.Empty;
    public string UserPrincipalName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string PrimarySmtp { get; set; } = string.Empty;
    public string SecondarySmtp { get; set; } = string.Empty;
    public string Company { get; set; } = "Automind";
    public string? Title { get; set; }
    public string? Department { get; set; }
    public string? Office { get; set; }
    public string? TelephoneNumber { get; set; }
    public string? ManagerDistinguishedName { get; set; }
    public string? OuDistinguishedName { get; set; }
    public List<string> Groups { get; set; } = [];
}

public sealed class AdProvisioningValidationResponse
{
    public bool Success { get; set; }
    public bool Valid { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AdValidationCheck> Checks { get; set; } = [];
    public List<GroupSuggestion> Groups { get; set; } = [];
    public AdProvisioningPreview? Preview { get; set; }
}
