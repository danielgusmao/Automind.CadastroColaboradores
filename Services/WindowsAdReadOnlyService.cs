using System.DirectoryServices;
using System.Runtime.Versioning;
using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

[SupportedOSPlatform("windows")]
public sealed class WindowsAdReadOnlyService(AdConnectionFactory directory) : IAdReadOnlyService
{
    public Task<IReadOnlyList<OrganizationalUnitOption>> GetOrganizationalUnitsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var root = directory.Open(directory.PeopleSearchBaseDn);
        using var searcher = new DirectorySearcher(root)
        {
            Filter = "(objectCategory=organizationalUnit)",
            SearchScope = SearchScope.Subtree,
            PageSize = 500
        };
        searcher.PropertiesToLoad.Add("distinguishedName");
        searcher.PropertiesToLoad.Add("ou");

        var allowed = directory.AllowedOuDns;
        var result = new List<OrganizationalUnitOption>();

        using var entries = searcher.FindAll();
        foreach (SearchResult entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dn = AdConnectionFactory.PropertyString(entry, "distinguishedName");
            if (string.IsNullOrWhiteSpace(dn) ||
                string.Equals(dn, directory.PeopleSearchBaseDn, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (allowed.Count > 0 && !allowed.Contains(dn)) continue;

            result.Add(new OrganizationalUnitOption
            {
                DistinguishedName = dn,
                DisplayName = AdConnectionFactory.ToOuDisplayPath(dn, directory.PeopleSearchBaseDn)
            });
        }

        IReadOnlyList<OrganizationalUnitOption> ordered = result
            .GroupBy(x => x.DistinguishedName, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => x.DisplayName, StringComparer.Create(new System.Globalization.CultureInfo("pt-BR"), true))
            .ToArray();

        return Task.FromResult(ordered);
    }

    public Task<AdIdentityAvailability> CheckIdentityAvailabilityAsync(
        string login,
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedLogin = (login ?? string.Empty).Trim();
        var normalizedEmail = (email ?? string.Empty).Trim();
        var primarySmtp = string.IsNullOrWhiteSpace(normalizedLogin)
            ? string.Empty
            : $"{normalizedLogin}@{directory.PrimarySmtpDomain}";

        var escapedLogin = AdConnectionFactory.EscapeFilter(normalizedLogin);
        var escapedEmail = AdConnectionFactory.EscapeFilter(normalizedEmail);
        var escapedPrimary = AdConnectionFactory.EscapeFilter(primarySmtp);

        var clauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(escapedLogin)) clauses.Add($"(sAMAccountName={escapedLogin})");
        if (!string.IsNullOrWhiteSpace(escapedEmail))
        {
            clauses.Add($"(userPrincipalName={escapedEmail})");
            clauses.Add($"(mail={escapedEmail})");
            clauses.Add($"(proxyAddresses=smtp:{escapedEmail})");
            clauses.Add($"(proxyAddresses=SMTP:{escapedEmail})");
        }
        if (!string.IsNullOrWhiteSpace(escapedPrimary))
        {
            clauses.Add($"(mail={escapedPrimary})");
            clauses.Add($"(proxyAddresses=smtp:{escapedPrimary})");
            clauses.Add($"(proxyAddresses=SMTP:{escapedPrimary})");
        }

        if (clauses.Count == 0)
        {
            return Task.FromResult(new AdIdentityAvailability());
        }

        using var root = directory.Open(directory.DomainBaseDn);
        using var searcher = new DirectorySearcher(root)
        {
            Filter = $"(|{string.Join(string.Empty, clauses)})",
            SearchScope = SearchScope.Subtree,
            PageSize = 500
        };
        foreach (var property in new[] { "name", "objectClass", "sAMAccountName", "userPrincipalName", "mail", "proxyAddresses", "distinguishedName" })
            searcher.PropertiesToLoad.Add(property);

        var loginAvailable = true;
        var upnAvailable = true;
        var emailAvailable = true;
        var collisions = new List<string>();

        using var entries = searcher.FindAll();
        foreach (SearchResult entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = AdConnectionFactory.PropertyString(entry, "name") ?? "objeto sem nome";
            var sam = AdConnectionFactory.PropertyString(entry, "sAMAccountName");
            var upn = AdConnectionFactory.PropertyString(entry, "userPrincipalName");
            var mail = AdConnectionFactory.PropertyString(entry, "mail");
            var proxies = AdConnectionFactory.PropertyStrings(entry, "proxyAddresses");
            var dn = AdConnectionFactory.PropertyString(entry, "distinguishedName") ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(normalizedLogin) && string.Equals(sam, normalizedLogin, StringComparison.OrdinalIgnoreCase))
                loginAvailable = false;

            if (!string.IsNullOrWhiteSpace(normalizedEmail) && string.Equals(upn, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                upnAvailable = false;

            var mailCollision =
                (!string.IsNullOrWhiteSpace(normalizedEmail) && string.Equals(mail, normalizedEmail, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(primarySmtp) && string.Equals(mail, primarySmtp, StringComparison.OrdinalIgnoreCase)) ||
                proxies.Any(x =>
                    string.Equals(StripSmtpPrefix(x), normalizedEmail, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(StripSmtpPrefix(x), primarySmtp, StringComparison.OrdinalIgnoreCase));

            if (mailCollision) emailAvailable = false;

            collisions.Add($"{name} ({sam ?? "sem sAMAccountName"}) - {dn}");
        }

        return Task.FromResult(new AdIdentityAvailability
        {
            LoginAvailable = loginAvailable,
            UpnAvailable = upnAvailable,
            EmailAvailable = emailAvailable,
            Collisions = collisions.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        });
    }

    public Task<AdCommonNameAvailability> CheckCommonNameAvailabilityAsync(
        string commonName,
        string organizationalUnitDistinguishedName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cn = (commonName ?? string.Empty).Trim();
        var ouDn = (organizationalUnitDistinguishedName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cn) || string.IsNullOrWhiteSpace(ouDn))
            return Task.FromResult(new AdCommonNameAvailability { Available = false });

        var allowed = directory.AllowedOuDns;
        if (allowed.Count > 0 && !allowed.Contains(ouDn))
            return Task.FromResult(new AdCommonNameAvailability { Available = false });

        try
        {
            using var root = directory.Open(ouDn);
            using var searcher = new DirectorySearcher(root)
            {
                Filter = $"(cn={AdConnectionFactory.EscapeFilter(cn)})",
                SearchScope = SearchScope.OneLevel,
                SizeLimit = 1
            };
            searcher.PropertiesToLoad.Add("distinguishedName");
            var match = searcher.FindOne();
            return Task.FromResult(new AdCommonNameAvailability
            {
                Available = match is null,
                ExistingDistinguishedName = match is null ? null : AdConnectionFactory.PropertyString(match, "distinguishedName")
            });
        }
        catch (DirectoryServicesCOMException)
        {
            return Task.FromResult(new AdCommonNameAvailability { Available = false });
        }
    }

    public Task<AdUserResolution> ResolveUserAsync(string value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var text = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text)) return Task.FromResult(new AdUserResolution());

        var escaped = AdConnectionFactory.EscapeFilter(text);
        using var root = directory.Open(directory.PeopleSearchBaseDn);
        using var searcher = new DirectorySearcher(root)
        {
            Filter = $"(&(objectCategory=person)(objectClass=user)(!(userAccountControl:1.2.840.113556.1.4.803:=2))(|(displayName={escaped})(cn={escaped})(sAMAccountName={escaped})))",
            SearchScope = SearchScope.Subtree,
            PageSize = 100
        };
        foreach (var property in new[] { "displayName", "sAMAccountName", "userPrincipalName", "distinguishedName" })
            searcher.PropertiesToLoad.Add(property);

        var matches = new List<AdUserResolution>();
        using var entries = searcher.FindAll();
        foreach (SearchResult entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            matches.Add(new AdUserResolution
            {
                Found = true,
                DisplayName = AdConnectionFactory.PropertyString(entry, "displayName"),
                SamAccountName = AdConnectionFactory.PropertyString(entry, "sAMAccountName"),
                UserPrincipalName = AdConnectionFactory.PropertyString(entry, "userPrincipalName"),
                DistinguishedName = AdConnectionFactory.PropertyString(entry, "distinguishedName")
            });
        }

        var distinct = matches
            .Where(x => !string.IsNullOrWhiteSpace(x.DistinguishedName))
            .GroupBy(x => x.DistinguishedName!, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();

        if (distinct.Length == 1)
        {
            distinct[0].Matches = 1;
            return Task.FromResult(distinct[0]);
        }

        return Task.FromResult(new AdUserResolution
        {
            Found = distinct.Length > 0,
            Ambiguous = distinct.Length > 1,
            Matches = distinct.Length
        });
    }

    public async Task<AdOuValidation> ValidateOrganizationalUnitAsync(
        string distinguishedName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dn = (distinguishedName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(dn)) return new AdOuValidation();

        var allowedOptions = await GetOrganizationalUnitsAsync(cancellationToken);
        var allowed = allowedOptions.FirstOrDefault(x => string.Equals(x.DistinguishedName, dn, StringComparison.OrdinalIgnoreCase));
        if (allowed is null) return new AdOuValidation { Allowed = false };

        try
        {
            using var root = directory.Open(dn);
            using var searcher = new DirectorySearcher(root)
            {
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.Base
            };
            searcher.PropertiesToLoad.Add("distinguishedName");
            var entry = searcher.FindOne();
            if (entry is null) return new AdOuValidation { Allowed = true, Exists = false };

            return new AdOuValidation
            {
                Exists = true,
                Allowed = true,
                DisplayName = allowed.DisplayName,
                DistinguishedName = dn
            };
        }
        catch (DirectoryServicesCOMException)
        {
            return new AdOuValidation { Allowed = true, Exists = false };
        }
    }

    public Task<AdGroupValidation> ValidateGroupsAsync(
        IEnumerable<string> distinguishedNames,
        CancellationToken cancellationToken = default)
    {
        var missing = new List<string>();

        foreach (var rawDn in distinguishedNames.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dn = rawDn.Trim();
            try
            {
                using var root = directory.Open(dn);
                using var searcher = new DirectorySearcher(root)
                {
                    Filter = "(objectCategory=group)",
                    SearchScope = SearchScope.Base
                };
                searcher.PropertiesToLoad.Add("distinguishedName");
                if (searcher.FindOne() is null) missing.Add(dn);
            }
            catch (DirectoryServicesCOMException)
            {
                missing.Add(dn);
            }
        }

        return Task.FromResult(new AdGroupValidation
        {
            AllExist = missing.Count == 0,
            MissingGroups = missing
        });
    }

    private static string StripSmtpPrefix(string value)
    {
        var index = value.IndexOf(':');
        return index >= 0 ? value[(index + 1)..] : value;
    }
}
