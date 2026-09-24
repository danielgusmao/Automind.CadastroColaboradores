using System.DirectoryServices;

namespace Automind.CadastroColaboradores.Services;

public sealed class AdConnectionFactory(IConfiguration configuration)
{
    public string Server => GetValue("Automind:Ad:Server", "10.1.2.1");
    public string DomainBaseDn => GetValue("Automind:Ad:BaseDn", "DC=automind,DC=com,DC=br");
    public string PeopleSearchBaseDn => GetValue("Automind:Ad:PeopleSearchBase", $"OU=Automind,{DomainBaseDn}");
    public string EmailDomain => GetValue("Automind:EmailDomain", "automind.com.br");
    public string PrimarySmtpDomain => GetValue("Automind:PrimarySmtpDomain", "automind.co");
    public string Mode => GetValue("Automind:Mode", "ReadOnly");
    public bool IsPilotWriteEnabled => string.Equals(Mode, "PilotWrite", StringComparison.OrdinalIgnoreCase);
    public string ExpectedTechnicalIdentity => GetValue("Automind:Provisioning:TechnicalIdentity", @"AUTOMIND\gMSA_CadColab$");
    public string ProvisioningAuditFile => GetValue("Automind:Provisioning:AuditFile", @"C:\Automind.CadastroColaboradores\Logs\ProvisioningAudit.jsonl");
    public int InitialPasswordLength => GetInt("Automind:Provisioning:InitialPasswordLength", 14, min: 14, max: 64);
    public bool GroupWritesEnabled => GetBool("Automind:Provisioning:GroupWritesEnabled", false);
    public bool PilotMembershipTestEnabled => GetBool("Automind:Provisioning:PilotMembershipTestEnabled", false);
    public string PilotMembershipUserDn => GetValue("Automind:Provisioning:PilotMembershipUserDn", string.Empty);
    public string PilotMembershipGroupDn => GetValue("Automind:Provisioning:PilotMembershipGroupDn", string.Empty);

    public IReadOnlySet<string> AllowedOuDns => GetSet("Automind:Ad:AllowedOuDns");
    public IReadOnlySet<string> WriteAllowedOuDns => GetSet("Automind:Provisioning:WriteAllowedOuDns");
    public IReadOnlySet<string> GroupWriteAllowedDns => GetSet("Automind:Provisioning:GroupWriteAllowedDns");
    public IReadOnlySet<string> ProtectedGroupNames => GetSet("Automind:Ad:ProtectedGroupNames");

    public DirectoryEntry Open(string distinguishedName)
        => new($"LDAP://{Server}/{distinguishedName}", null, null, AuthenticationTypes.Secure);

    public static string EscapeFilter(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        return value
            .Replace("\\", "\\5c", StringComparison.Ordinal)
            .Replace("*", "\\2a", StringComparison.Ordinal)
            .Replace("(", "\\28", StringComparison.Ordinal)
            .Replace(")", "\\29", StringComparison.Ordinal)
            .Replace("\0", "\\00", StringComparison.Ordinal);
    }

    public static IReadOnlySet<string> ReadStringSet(IConfiguration configuration, string key)
        => configuration
            .GetSection(key)
            .GetChildren()
            .Select(x => x.Value?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static string? PropertyString(SearchResult result, string name)
    {
        if (!result.Properties.Contains(name) || result.Properties[name].Count == 0) return null;
        return result.Properties[name][0]?.ToString();
    }

    public static IReadOnlyList<string> PropertyStrings(SearchResult result, string name)
    {
        if (!result.Properties.Contains(name) || result.Properties[name].Count == 0) return [];
        return result.Properties[name].Cast<object>().Select(x => x?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToArray();
    }

    public static int? PropertyInt(SearchResult result, string name)
    {
        var value = PropertyString(result, name);
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    public static bool PropertyBool(SearchResult result, string name)
    {
        var value = PropertyString(result, name);
        return bool.TryParse(value, out var parsed) && parsed;
    }

    public static string ToOuDisplayPath(string distinguishedName, string peopleSearchBaseDn)
    {
        var baseOu = peopleSearchBaseDn.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(x => x.StartsWith("OU=", StringComparison.OrdinalIgnoreCase));
        var baseName = baseOu?.Length > 3 ? baseOu[3..] : null;

        var ous = distinguishedName
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.StartsWith("OU=", StringComparison.OrdinalIgnoreCase))
            .Select(x => x[3..])
            .Where(x => !string.Equals(x, baseName, StringComparison.OrdinalIgnoreCase))
            .Reverse()
            .ToArray();

        return ous.Length == 0 ? distinguishedName : string.Join('/', ous);
    }

    private IReadOnlySet<string> GetSet(string key) => ReadStringSet(configuration, key);

    private string GetValue(string key, string fallback)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private bool GetBool(string key, bool fallback)
    {
        var value = configuration[key];
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private int GetInt(string key, int fallback, int min, int max)
    {
        var value = configuration[key];
        if (!int.TryParse(value, out var parsed)) return fallback;
        return Math.Clamp(parsed, min, max);
    }
}
