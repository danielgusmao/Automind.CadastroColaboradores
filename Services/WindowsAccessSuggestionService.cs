using System.DirectoryServices;
using System.Runtime.Versioning;
using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

[SupportedOSPlatform("windows")]
public sealed class WindowsAccessSuggestionService(AdConnectionFactory directory, IConfiguration configuration) : IAccessSuggestionService
{
    private const int SecurityEnabledFlag = unchecked((int)0x80000000);
    private const int GlobalScopeFlag = 0x00000002;
    private const int DomainLocalScopeFlag = 0x00000004;
    private const int UniversalScopeFlag = 0x00000008;

    public Task<IReadOnlyList<GroupSuggestion>> SuggestAsync(
        string? cargo,
        string? departamento,
        string? excludedSamAccountName,
        string? excludedCommonName,
        string? excludedOuDistinguishedName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedCargo = (cargo ?? string.Empty).Trim();
        var normalizedDepartment = (departamento ?? string.Empty).Trim();
        var normalizedExcludedSam = (excludedSamAccountName ?? string.Empty).Trim();
        var normalizedExcludedCommonName = (excludedCommonName ?? string.Empty).Trim();
        var normalizedExcludedOuDn = (excludedOuDistinguishedName ?? string.Empty).Trim();
        var referenceExcludedOuDns = configuration.GetSection("Automind:Ad:SuggestionExcludedOuDns").GetChildren()
            .Select(x => (x.Value ?? string.Empty).Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        if (string.IsNullOrWhiteSpace(normalizedCargo) || string.IsNullOrWhiteSpace(normalizedDepartment))
            return Task.FromResult<IReadOnlyList<GroupSuggestion>>([]);

        var escapedCargo = AdConnectionFactory.EscapeFilter(normalizedCargo);
        var escapedDepartment = AdConnectionFactory.EscapeFilter(normalizedDepartment);
        var exclusion = string.IsNullOrWhiteSpace(normalizedExcludedSam)
            ? string.Empty
            : $"(!(sAMAccountName={AdConnectionFactory.EscapeFilter(normalizedExcludedSam)}))";

        using var root = directory.Open(directory.PeopleSearchBaseDn);
        using var searcher = new DirectorySearcher(root)
        {
            Filter = $"(&(objectCategory=person)(objectClass=user)(!(userAccountControl:1.2.840.113556.1.4.803:=2)){exclusion}(title={escapedCargo})(department={escapedDepartment}))",
            SearchScope = SearchScope.Subtree,
            PageSize = 500
        };
        searcher.PropertiesToLoad.Add("memberOf");
        searcher.PropertiesToLoad.Add("sAMAccountName");
        searcher.PropertiesToLoad.Add("cn");
        searcher.PropertiesToLoad.Add("distinguishedName");

        var users = new List<IReadOnlyList<string>>();
        using var entries = searcher.FindAll();
        foreach (SearchResult entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sam = AdConnectionFactory.PropertyString(entry, "sAMAccountName") ?? string.Empty;
            var cn = AdConnectionFactory.PropertyString(entry, "cn") ?? string.Empty;
            var dn = AdConnectionFactory.PropertyString(entry, "distinguishedName") ?? string.Empty;

            if (referenceExcludedOuDns.Any(excludedOu => IsUnderOu(dn, excludedOu)))
                continue;

            if (!string.IsNullOrWhiteSpace(normalizedExcludedSam)
                && string.Equals(sam, normalizedExcludedSam, StringComparison.OrdinalIgnoreCase))
                continue;

            var firstComma = dn.IndexOf(',');
            var parentDn = firstComma >= 0 && firstComma + 1 < dn.Length ? dn[(firstComma + 1)..] : string.Empty;
            if (!string.IsNullOrWhiteSpace(normalizedExcludedCommonName)
                && !string.IsNullOrWhiteSpace(normalizedExcludedOuDn)
                && string.Equals(cn, normalizedExcludedCommonName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(parentDn, normalizedExcludedOuDn, StringComparison.OrdinalIgnoreCase))
                continue;

            users.Add(AdConnectionFactory.PropertyStrings(entry, "memberOf"));
        }

        if (users.Count == 0) return Task.FromResult<IReadOnlyList<GroupSuggestion>>([]);

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var memberships in users)
        {
            foreach (var groupDn in memberships.Distinct(StringComparer.OrdinalIgnoreCase))
                counts[groupDn] = counts.GetValueOrDefault(groupDn) + 1;
        }

        var suggestions = new List<GroupSuggestion>();
        foreach (var pair in counts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var metadata = GetGroupMetadata(pair.Key, cancellationToken);
            if (metadata is null) continue;

            var common = pair.Value == users.Count;
            var protectedGroup = metadata.Protected || metadata.AncestorProtected;

            suggestions.Add(ToSuggestion(metadata, pair.Key, pair.Value, users.Count, common && !protectedGroup));
        }

        IReadOnlyList<GroupSuggestion> result = suggestions
            .OrderByDescending(x => x.Selecionado)
            .ThenBy(x => x.Protegido)
            .ThenByDescending(x => x.EncontradoEm)
            .ThenBy(x => x.Nome, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<GroupSuggestion>> SearchGroupsAsync(
        string? query,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = (query ?? string.Empty).Trim();
        if (normalized.Length < 2)
            return Task.FromResult<IReadOnlyList<GroupSuggestion>>([]);

        limit = Math.Clamp(limit, 1, 50);
        var escaped = AdConnectionFactory.EscapeFilter(normalized);

        using var root = directory.Open(directory.DomainBaseDn);
        using var searcher = new DirectorySearcher(root)
        {
            Filter = $"(&(objectCategory=group)(|(sAMAccountName=*{escaped}*)(name=*{escaped}*)))",
            SearchScope = SearchScope.Subtree,
            PageSize = 100,
            SizeLimit = limit
        };
        searcher.PropertiesToLoad.Add("distinguishedName");

        var results = new List<GroupSuggestion>();
        using var entries = searcher.FindAll();
        foreach (SearchResult entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dn = AdConnectionFactory.PropertyString(entry, "distinguishedName");
            if (string.IsNullOrWhiteSpace(dn)) continue;

            var metadata = GetGroupMetadata(dn, cancellationToken);
            if (metadata is null) continue;
            results.Add(ToSuggestion(metadata, dn, 0, 0, false));
        }

        IReadOnlyList<GroupSuggestion> ordered = results
            .GroupBy(x => x.DistinguishedName, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => x.Protegido)
            .ThenBy(x => x.Nome, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToArray();

        return Task.FromResult(ordered);
    }

    public Task<IReadOnlyList<GroupSuggestion>> ResolveGroupsAsync(
        IEnumerable<string> distinguishedNames,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = new List<GroupSuggestion>();

        foreach (var rawDn in distinguishedNames
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Select(x => x.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var metadata = GetGroupMetadata(rawDn, cancellationToken);
            if (metadata is null) continue;
            results.Add(ToSuggestion(metadata, rawDn, 0, 0, false));
        }

        return Task.FromResult<IReadOnlyList<GroupSuggestion>>(results);
    }

    private GroupSuggestion ToSuggestion(
        GroupMetadata metadata,
        string distinguishedName,
        int encontradoEm,
        int totalComparados,
        bool selecionado)
    {
        var protectedGroup = metadata.Protected || metadata.AncestorProtected;
        return new GroupSuggestion
        {
            Nome = metadata.Name,
            DistinguishedName = distinguishedName,
            EncontradoEm = encontradoEm,
            TotalComparados = totalComparados,
            Selecionado = selecionado && !protectedGroup,
            Protegido = protectedGroup,
            Categoria = metadata.Category,
            Escopo = metadata.Scope,
            EfeitosIndiretos = metadata.Ancestors
        };
    }

    private GroupMetadata? GetGroupMetadata(string distinguishedName, CancellationToken cancellationToken)
    {
        try
        {
            using var root = directory.Open(distinguishedName);
            using var searcher = new DirectorySearcher(root)
            {
                Filter = "(objectCategory=group)",
                SearchScope = SearchScope.Base
            };
            foreach (var property in new[] { "sAMAccountName", "name", "groupType", "adminCount", "isCriticalSystemObject", "distinguishedName" })
                searcher.PropertiesToLoad.Add(property);

            var entry = searcher.FindOne();
            if (entry is null) return null;

            var name = AdConnectionFactory.PropertyString(entry, "sAMAccountName")
                       ?? AdConnectionFactory.PropertyString(entry, "name")
                       ?? distinguishedName;
            var groupType = AdConnectionFactory.PropertyInt(entry, "groupType") ?? 0;
            var adminCount = AdConnectionFactory.PropertyInt(entry, "adminCount") ?? 0;
            var critical = AdConnectionFactory.PropertyBool(entry, "isCriticalSystemObject");
            var category = IsSecurity(groupType) ? "Security" : "Distribution";
            var scope = GetScope(groupType);

            var ancestors = IsSecurity(groupType)
                ? GetSecurityAncestors(distinguishedName, cancellationToken)
                : Array.Empty<AncestorGroup>();
            var protectedNames = directory.ProtectedGroupNames;
            var ownProtected = adminCount == 1 || critical || protectedNames.Contains(name) || distinguishedName.Contains(",CN=Builtin,", StringComparison.OrdinalIgnoreCase);
            var ancestorProtected = ancestors.Any(x => x.Protected);

            return new GroupMetadata(
                name,
                category,
                scope,
                ownProtected,
                ancestorProtected,
                ancestors.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList());
        }
        catch (DirectoryServicesCOMException)
        {
            return null;
        }
    }

    private IReadOnlyList<AncestorGroup> GetSecurityAncestors(string sourceDn, CancellationToken cancellationToken)
    {
        var escapedDn = AdConnectionFactory.EscapeFilter(sourceDn);
        using var root = directory.Open(directory.DomainBaseDn);
        using var searcher = new DirectorySearcher(root)
        {
            Filter = $"(&(objectCategory=group)(member:1.2.840.113556.1.4.1941:={escapedDn}))",
            SearchScope = SearchScope.Subtree,
            PageSize = 500
        };
        foreach (var property in new[] { "sAMAccountName", "name", "groupType", "adminCount", "isCriticalSystemObject", "distinguishedName" })
            searcher.PropertiesToLoad.Add(property);

        var ancestors = new List<AncestorGroup>();
        using var entries = searcher.FindAll();
        foreach (SearchResult entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var groupType = AdConnectionFactory.PropertyInt(entry, "groupType") ?? 0;
            if (!IsSecurity(groupType)) continue;

            var name = AdConnectionFactory.PropertyString(entry, "sAMAccountName")
                       ?? AdConnectionFactory.PropertyString(entry, "name")
                       ?? "grupo sem nome";
            var dn = AdConnectionFactory.PropertyString(entry, "distinguishedName") ?? string.Empty;
            var adminCount = AdConnectionFactory.PropertyInt(entry, "adminCount") ?? 0;
            var critical = AdConnectionFactory.PropertyBool(entry, "isCriticalSystemObject");
            var protectedGroup = adminCount == 1 || critical || directory.ProtectedGroupNames.Contains(name) || dn.Contains(",CN=Builtin,", StringComparison.OrdinalIgnoreCase);

            ancestors.Add(new AncestorGroup(name, protectedGroup));
        }

        return ancestors;
    }

    private static bool IsSecurity(int groupType) => (groupType & SecurityEnabledFlag) == SecurityEnabledFlag;

    private static string GetScope(int groupType)
    {
        if ((groupType & UniversalScopeFlag) == UniversalScopeFlag) return "Universal";
        if ((groupType & DomainLocalScopeFlag) == DomainLocalScopeFlag) return "DomainLocal";
        if ((groupType & GlobalScopeFlag) == GlobalScopeFlag) return "Global";
        return "Desconhecido";
    }

    private sealed record GroupMetadata(
        string Name,
        string Category,
        string Scope,
        bool Protected,
        bool AncestorProtected,
        List<string> Ancestors);

    private sealed record AncestorGroup(string Name, bool Protected);
    private static bool IsUnderOu(string distinguishedName, string ouDistinguishedName)
    {
        if (string.IsNullOrWhiteSpace(distinguishedName) || string.IsNullOrWhiteSpace(ouDistinguishedName))
            return false;

        return distinguishedName.EndsWith("," + ouDistinguishedName, StringComparison.OrdinalIgnoreCase);
    }

}
