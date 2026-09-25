using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Automind.CadastroColaboradores.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Automind.CadastroColaboradores.Services;

public sealed class Microsoft365LicenseService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IProvisioningAuditService audit,
    ILogger<Microsoft365LicenseService> logger) : IMicrosoft365LicenseService
{
    private const string CacheKey = "m365:subscribed-skus";
    private const int LicenseReadbackAttempts = 12;
    private static readonly TimeSpan LicenseReadbackDelay = TimeSpan.FromMilliseconds(1500);

    private static readonly IReadOnlyDictionary<string, string> DisplayNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CCIBOTS_PRIVPREV_VIRAL"] = "Microsoft Copilot Studio Viral Trial",
            ["CPC_B_2C_8RAM_128GB"] = "Windows 365 Business 2 vCPU, 8 GB, 128 GB",
            ["CPC_B_2C_8RAM_128GB_WHB"] = "Windows 365 Business 2 vCPU, 8 GB, 128 GB (Windows Hybrid Benefit)",
            ["ENTERPRISEPACK"] = "Office 365 E3",
            ["EXCHANGEDESKLESS"] = "Exchange Online Kiosk",
            ["EXCHANGESTANDARD"] = "Exchange Online (Plan 1)",
            ["FLOW_FREE"] = "Microsoft Power Automate Free",
            ["FLOW_PER_USER"] = "Power Automate per user",
            ["Microsoft_Teams_Rooms_Pro"] = "Microsoft Teams Rooms Pro",
            ["O365_BUSINESS_ESSENTIALS"] = "Microsoft 365 Business Basic",
            ["O365_BUSINESS_PREMIUM"] = "Microsoft 365 Business Standard",
            ["POWER_BI_PRO"] = "Power BI Pro",
            ["POWER_BI_STANDARD"] = "Microsoft Fabric (Free)",
            ["POWERAPPS_DEV"] = "Microsoft Power Apps for Developer",
            ["PROJECT_P1"] = "Project Plan 1",
            ["PROJECT_PLAN3_DEPT"] = "Project Plan 3 (Department)",
            ["PROJECTPROFESSIONAL"] = "Project Plan 3",
            ["TEAMS_ESSENTIALS_AAD"] = "Microsoft Teams Essentials",
            ["Teams_Premium_(for_Departments)"] = "Microsoft Teams Premium (for Departments)",
            ["WINDOWS_STORE"] = "Windows Store for Business"
        };

    private IConfigurationSection Settings => configuration.GetSection("Automind:Microsoft365");
    private string TechnicalIdentity => configuration["Automind:Provisioning:TechnicalIdentity"]?.Trim() ?? "AUTOMIND\\gMSA_CadColab$";
    private string DesiredUsageLocation => string.IsNullOrWhiteSpace(Settings["UsageLocation"]) ? "BR" : Settings["UsageLocation"]!.Trim().ToUpperInvariant();

    public bool IsEnabled => Settings.GetValue<bool>("Enabled") && Settings.GetValue<bool>("LicenseInventoryEnabled");
    public bool LicenseWritesEnabled => Settings.GetValue<bool>("LicenseWritesEnabled");
    public int SyncPollSeconds => Math.Clamp(Settings.GetValue<int?>("SyncPollSeconds") ?? 10, 5, 60);
    public int SyncMaxWaitSeconds => Math.Clamp(Settings.GetValue<int?>("SyncMaxWaitSeconds") ?? 180, 30, 600);

    public Task<IReadOnlyList<Microsoft365LicenseInfo>> GetSubscribedLicensesAsync(CancellationToken cancellationToken = default) =>
        GetSubscribedLicensesCoreAsync(forceRefresh: false, cancellationToken);

    public async Task<Microsoft365LicenseSelectionValidation> ValidateSelectionAsync(
        IEnumerable<Guid> skuIds,
        CancellationToken cancellationToken = default)
    {
        var selectedIds = NormalizeSkuIds(skuIds);
        if (selectedIds.Count == 0)
        {
            return new Microsoft365LicenseSelectionValidation
            {
                Valid = true,
                Message = "Nenhuma licença Microsoft 365 selecionada."
            };
        }

        if (!IsEnabled)
        {
            return new Microsoft365LicenseSelectionValidation
            {
                Valid = false,
                Message = "A integração Microsoft 365 está desativada."
            };
        }

        if (!LicenseWritesEnabled)
        {
            return new Microsoft365LicenseSelectionValidation
            {
                Valid = false,
                Message = "A escrita de licenças Microsoft 365 está desabilitada."
            };
        }

        var inventory = await GetSubscribedLicensesCoreAsync(forceRefresh: true, cancellationToken);
        var byId = inventory.Where(x => x.SkuId != Guid.Empty).ToDictionary(x => x.SkuId);
        var selected = new List<Microsoft365LicenseInfo>();
        var errors = new List<string>();

        foreach (var id in selectedIds)
        {
            if (!byId.TryGetValue(id, out var license))
            {
                errors.Add($"SKU {id} não pertence ao inventário atual do tenant.");
                continue;
            }

            selected.Add(license);
            if (license.IsSuspended)
                errors.Add($"{license.DisplayName}: assinatura suspensa.");
            else if (!license.IsUnlimited && license.AvailableUnits <= 0)
                errors.Add($"{license.DisplayName}: sem licenças disponíveis.");
        }

        return new Microsoft365LicenseSelectionValidation
        {
            Valid = errors.Count == 0,
            Message = errors.Count == 0
                ? $"{selected.Count} licença(s) Microsoft 365 selecionada(s) e disponível(is)."
                : string.Join(" ", errors),
            SelectedLicenses = selected
        };
    }

    public async Task<Microsoft365LicenseAssignmentResponse> AssignLicensesAsync(
        string chamado,
        string operatorName,
        string userPrincipalName,
        IEnumerable<Guid> skuIds,
        CancellationToken cancellationToken = default)
    {
        var selectedIds = NormalizeSkuIds(skuIds);
        var upn = (userPrincipalName ?? string.Empty).Trim();
        var ticket = (chamado ?? string.Empty).Trim().ToUpperInvariant();

        if (!IsEnabled || !LicenseWritesEnabled)
            return Failed(upn, "A escrita de licenças Microsoft 365 está desabilitada.");
        if (string.IsNullOrWhiteSpace(upn))
            return Failed(upn, "UPN do usuário não informado.");
        if (selectedIds.Count == 0)
            return new Microsoft365LicenseAssignmentResponse { Success = true, UserPrincipalName = upn, Message = "Nenhuma licença Microsoft 365 selecionada." };

        var client = httpClientFactory.CreateClient("MicrosoftGraph");
        var token = await AcquireAccessTokenAsync(client, cancellationToken);
        var user = await ReadUserAsync(client, token, upn, cancellationToken);
        if (user is null)
        {
            logger.LogInformation("Usuário {UserPrincipalName} ainda não disponível no Entra. Aguardando Cloud Sync.", upn);
            return new Microsoft365LicenseAssignmentResponse
            {
                Success = false,
                PendingSynchronization = true,
                UserPrincipalName = upn,
                RetryAfterSeconds = SyncPollSeconds,
                Message = "Usuário criado no AD, mas ainda não sincronizado com o Microsoft Entra. Nenhuma licença foi atribuída."
            };
        }

        if (user.OnPremisesSyncEnabled != true)
            return Failed(upn, "O objeto localizado no Entra não está marcado como sincronizado do Active Directory. Escrita bloqueada para revisão.");

        var alreadyAssigned = user.AssignedSkuIds.ToHashSet();
        var toAdd = selectedIds.Where(id => !alreadyAssigned.Contains(id)).ToList();
        var inventory = await GetSubscribedLicensesCoreAsync(forceRefresh: true, cancellationToken);
        var byId = inventory.Where(x => x.SkuId != Guid.Empty).ToDictionary(x => x.SkuId);
        var selectedNames = selectedIds.Select(id => byId.TryGetValue(id, out var item) ? item.DisplayName : id.ToString()).ToList();

        var unavailable = toAdd
            .Where(id => !byId.TryGetValue(id, out var license) || !license.CanAssign)
            .Select(id => byId.TryGetValue(id, out var license) ? license.DisplayName : id.ToString())
            .ToList();
        if (unavailable.Count > 0)
            return Failed(upn, $"Licença(s) indisponível(is) no momento da gravação: {string.Join(", ", unavailable)}.");

        var operationId = Guid.NewGuid().ToString("N");
        var steps = new List<Microsoft365LicenseAssignmentStepResult>();
        var assignmentAttempted = false;
        var usageLocationChanged = false;

        await AuditAsync(operationId, ticket, operatorName, "m365-license-start", "started", upn, [], selectedNames, cancellationToken);

        try
        {
            if (string.IsNullOrWhiteSpace(user.UsageLocation))
            {
                await UpdateUsageLocationAsync(client, token, upn, DesiredUsageLocation, cancellationToken);
                usageLocationChanged = true;
                user = await ReadUserAsync(client, token, upn, cancellationToken)
                    ?? throw new InvalidOperationException("Usuário deixou de ser localizado após atualizar UsageLocation.");
                if (!string.Equals(user.UsageLocation, DesiredUsageLocation, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("O Microsoft Graph não confirmou UsageLocation após a atualização.");

                steps.Add(Step("usage-location", true, $"UsageLocation definido como {DesiredUsageLocation}."));
                await AuditAsync(operationId, ticket, operatorName, "m365-usage-location", "success", upn, ["usageLocation"], [], cancellationToken);
            }
            else if (!string.Equals(user.UsageLocation, DesiredUsageLocation, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"UsageLocation atual é {user.UsageLocation}; o fluxo automático não sobrescreve um país já definido.");
            }
            else
            {
                steps.Add(Step("usage-location", true, $"UsageLocation já estava em {DesiredUsageLocation}."));
            }

            if (toAdd.Count == 0)
            {
                steps.Add(Step("licenses", true, "Todas as licenças selecionadas já estavam atribuídas diretamente ao usuário."));
                await AuditAsync(operationId, ticket, operatorName, "m365-license-complete", "success", upn, [], selectedNames, cancellationToken);
                return new Microsoft365LicenseAssignmentResponse
                {
                    Success = true,
                    Changed = usageLocationChanged,
                    UserPrincipalName = upn,
                    UsageLocation = user.UsageLocation,
                    Message = "Microsoft 365 validado; as licenças selecionadas já estavam atribuídas.",
                    AssignedLicenseNames = selectedNames,
                    Steps = steps
                };
            }

            assignmentAttempted = true;
            await AssignLicenseDeltaAsync(client, token, upn, toAdd, [], cancellationToken);
            cache.Remove(CacheKey);
            steps.Add(Step("licenses", true, $"Solicitação de atribuição enviada para {toAdd.Count} licença(s)."));
            await AuditAsync(operationId, ticket, operatorName, "m365-license-assign", "success", upn, [], NamesFor(toAdd, byId), cancellationToken);

            var readback = await WaitForLicenseStateAsync(
                client, token, upn,
                snapshot => selectedIds.All(id => snapshot.AssignedSkuIds.Contains(id)),
                cancellationToken);

            if (readback is null || !selectedIds.All(id => readback.AssignedSkuIds.Contains(id)))
                throw new InvalidOperationException("O readback do Microsoft Graph não confirmou todas as licenças selecionadas.");

            var stateErrors = readback.LicenseStates
                .Where(x => selectedIds.Contains(x.SkuId) && !string.IsNullOrWhiteSpace(x.Error) && !string.Equals(x.Error, "None", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (stateErrors.Count > 0)
                throw new InvalidOperationException("O Graph retornou erro no estado de uma ou mais licenças após a atribuição.");

            steps.Add(Step("readback", true, "Readback confirmou as licenças selecionadas no usuário."));
            await AuditAsync(operationId, ticket, operatorName, "m365-license-readback", "success", upn, [], selectedNames, cancellationToken);
            await AuditAsync(operationId, ticket, operatorName, "m365-license-complete", "success", upn, usageLocationChanged ? new[] { "usageLocation" } : Array.Empty<string>(), selectedNames, cancellationToken);

            return new Microsoft365LicenseAssignmentResponse
            {
                Success = true,
                Changed = true,
                UserPrincipalName = upn,
                UsageLocation = readback.UsageLocation,
                Message = $"Microsoft 365 concluído: {toAdd.Count} licença(s) atribuída(s) e confirmada(s).",
                AddedSkuIds = toAdd,
                AssignedLicenseNames = selectedNames,
                Steps = steps
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning("Falha na atribuição Microsoft 365 para {UserPrincipalName}. Tipo: {ErrorType}; código: {Code}", upn, exception.GetType().Name, exception.HResult);
            var rollbackOk = true;

            if (assignmentAttempted && toAdd.Count > 0)
            {
                try
                {
                    await AssignLicenseDeltaAsync(client, token, upn, [], toAdd, cancellationToken);
                    cache.Remove(CacheKey);
                    var rollbackReadback = await WaitForLicenseStateAsync(
                        client, token, upn,
                        snapshot => toAdd.All(id => !snapshot.AssignedSkuIds.Contains(id)),
                        cancellationToken);
                    rollbackOk = rollbackReadback is not null && toAdd.All(id => !rollbackReadback.AssignedSkuIds.Contains(id));
                    steps.Add(Step("rollback", rollbackOk, rollbackOk
                        ? "Rollback removeu somente as licenças adicionadas nesta operação."
                        : "Rollback enviado, mas o readback ainda encontrou licença adicionada nesta operação."));
                    await AuditAsync(operationId, ticket, operatorName, "m365-license-rollback", rollbackOk ? "success" : "failed", upn, [], NamesFor(toAdd, byId), cancellationToken, rollbackOk ? null : "ReadbackRollbackFailed", rollbackOk ? null : -1);
                }
                catch (Exception rollbackException) when (rollbackException is not OperationCanceledException)
                {
                    rollbackOk = false;
                    logger.LogError(rollbackException, "Falha no rollback de licenças Microsoft 365 para {UserPrincipalName}.", upn);
                    steps.Add(Step("rollback", false, "Falha ao remover as licenças adicionadas nesta operação."));
                    await AuditAsync(operationId, ticket, operatorName, "m365-license-rollback", "failed", upn, [], NamesFor(toAdd, byId), cancellationToken, rollbackException.GetType().Name, rollbackException.HResult);
                }
            }

            await AuditAsync(operationId, ticket, operatorName, "m365-license-complete", "failed", upn, usageLocationChanged ? new[] { "usageLocation" } : Array.Empty<string>(), selectedNames, cancellationToken, exception.GetType().Name, exception.HResult);

            return new Microsoft365LicenseAssignmentResponse
            {
                Success = false,
                Changed = usageLocationChanged,
                RequiresManualReview = !rollbackOk,
                UserPrincipalName = upn,
                UsageLocation = usageLocationChanged ? DesiredUsageLocation : user.UsageLocation,
                Message = rollbackOk
                    ? "A atribuição Microsoft 365 não foi concluída. As licenças adicionadas nesta tentativa foram revertidas; UsageLocation pode permanecer em BR."
                    : "Falha na atribuição Microsoft 365 e o rollback não pôde ser confirmado. Revisão manual obrigatória.",
                AssignedLicenseNames = selectedNames,
                Steps = steps
            };
        }
    }

    private async Task<IReadOnlyList<Microsoft365LicenseInfo>> GetSubscribedLicensesCoreAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!IsEnabled)
            return [];

        if (!forceRefresh && cache.TryGetValue(CacheKey, out IReadOnlyList<Microsoft365LicenseInfo>? cached) && cached is not null)
            return cached;

        var client = httpClientFactory.CreateClient("MicrosoftGraph");
        var token = await AcquireAccessTokenAsync(client, cancellationToken);
        using var request = Authorized(HttpMethod.Get,
            "https://graph.microsoft.com/v1.0/subscribedSkus?$select=skuId,skuPartNumber,consumedUnits,prepaidUnits,capabilityStatus",
            token);
        request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Falha ao consultar subscribedSkus no Microsoft Graph. HTTP {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException($"Microsoft Graph retornou HTTP {(int)response.StatusCode} ao consultar licenças.");
        }

        using var document = JsonDocument.Parse(content);
        var result = new List<Microsoft365LicenseInfo>();
        if (document.RootElement.TryGetProperty("value", out var values) && values.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in values.EnumerateArray())
            {
                var skuPartNumber = item.TryGetProperty("skuPartNumber", out var part) ? part.GetString() ?? string.Empty : string.Empty;
                var skuId = item.TryGetProperty("skuId", out var id) && Guid.TryParse(id.GetString(), out var parsedId) ? parsedId : Guid.Empty;
                var consumed = item.TryGetProperty("consumedUnits", out var consumedElement) && consumedElement.TryGetInt32(out var consumedValue) ? consumedValue : 0;
                var capabilityStatus = item.TryGetProperty("capabilityStatus", out var status) ? status.GetString() ?? string.Empty : string.Empty;

                var enabled = 0;
                var suspended = 0;
                if (item.TryGetProperty("prepaidUnits", out var prepaid) && prepaid.ValueKind == JsonValueKind.Object)
                {
                    if (prepaid.TryGetProperty("enabled", out var enabledElement) && enabledElement.TryGetInt32(out var enabledValue))
                        enabled = enabledValue;
                    if (prepaid.TryGetProperty("suspended", out var suspendedElement) && suspendedElement.TryGetInt32(out var suspendedValue))
                        suspended = suspendedValue;
                }

                result.Add(new Microsoft365LicenseInfo
                {
                    SkuId = skuId,
                    SkuPartNumber = skuPartNumber,
                    DisplayName = DisplayNames.TryGetValue(skuPartNumber, out var displayName) ? displayName : FriendlyFallback(skuPartNumber),
                    EnabledUnits = enabled,
                    ConsumedUnits = consumed,
                    SuspendedUnits = suspended,
                    CapabilityStatus = capabilityStatus
                });
            }
        }

        var ordered = result
            .OrderBy(x => x.IsSuspended)
            .ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        var cacheMinutes = Math.Clamp(Settings.GetValue<int?>("InventoryCacheMinutes") ?? 5, 1, 60);
        cache.Set(CacheKey, ordered, TimeSpan.FromMinutes(cacheMinutes));
        return ordered;
    }


    private async Task<GraphUserSnapshot?> WaitForLicenseStateAsync(
        HttpClient client,
        string token,
        string userPrincipalName,
        Func<GraphUserSnapshot, bool> predicate,
        CancellationToken cancellationToken)
    {
        GraphUserSnapshot? snapshot = null;
        for (var attempt = 0; attempt < LicenseReadbackAttempts; attempt++)
        {
            snapshot = await ReadUserAsync(client, token, userPrincipalName, cancellationToken);
            if (snapshot is not null && predicate(snapshot))
                return snapshot;

            if (attempt < LicenseReadbackAttempts - 1)
                await Task.Delay(LicenseReadbackDelay, cancellationToken);
        }

        return snapshot;
    }

    private async Task<GraphUserSnapshot?> ReadUserAsync(HttpClient client, string token, string userPrincipalName, CancellationToken cancellationToken)
    {
        var encoded = Uri.EscapeDataString(userPrincipalName);
        using var request = Authorized(HttpMethod.Get,
            $"https://graph.microsoft.com/v1.0/users/{encoded}?$select=id,displayName,userPrincipalName,usageLocation,onPremisesSyncEnabled,assignedLicenses,licenseAssignmentStates",
            token);
        request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Microsoft Graph retornou HTTP {(int)response.StatusCode} ao consultar o usuário no Entra.");

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        var assigned = new HashSet<Guid>();
        if (root.TryGetProperty("assignedLicenses", out var assignedElement) && assignedElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in assignedElement.EnumerateArray())
                if (item.TryGetProperty("skuId", out var sku) && Guid.TryParse(sku.GetString(), out var id))
                    assigned.Add(id);
        }

        var states = new List<GraphLicenseState>();
        if (root.TryGetProperty("licenseAssignmentStates", out var stateElement) && stateElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in stateElement.EnumerateArray())
            {
                if (!item.TryGetProperty("skuId", out var sku) || !Guid.TryParse(sku.GetString(), out var id))
                    continue;
                states.Add(new GraphLicenseState(
                    id,
                    item.TryGetProperty("state", out var state) ? state.GetString() : null,
                    item.TryGetProperty("error", out var error) ? error.GetString() : null));
            }
        }

        return new GraphUserSnapshot(
            root.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() ?? userPrincipalName : userPrincipalName,
            root.TryGetProperty("usageLocation", out var location) ? location.GetString() : null,
            root.TryGetProperty("onPremisesSyncEnabled", out var syncEnabled) && (syncEnabled.ValueKind == JsonValueKind.True || syncEnabled.ValueKind == JsonValueKind.False) ? syncEnabled.GetBoolean() : null,
            assigned,
            states);
    }

    private static async Task UpdateUsageLocationAsync(HttpClient client, string token, string userPrincipalName, string usageLocation, CancellationToken cancellationToken)
    {
        var encoded = Uri.EscapeDataString(userPrincipalName);
        using var request = Authorized(HttpMethod.Patch, $"https://graph.microsoft.com/v1.0/users/{encoded}", token);
        request.Content = JsonBody(new { usageLocation });
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Microsoft Graph retornou HTTP {(int)response.StatusCode} ao atualizar UsageLocation.");
    }

    private static async Task AssignLicenseDeltaAsync(
        HttpClient client,
        string token,
        string userPrincipalName,
        IReadOnlyCollection<Guid> addSkuIds,
        IReadOnlyCollection<Guid> removeSkuIds,
        CancellationToken cancellationToken)
    {
        var encoded = Uri.EscapeDataString(userPrincipalName);
        using var request = Authorized(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/users/{encoded}/assignLicense", token);
        request.Content = JsonBody(new
        {
            addLicenses = addSkuIds.Select(id => new { skuId = id, disabledPlans = Array.Empty<Guid>() }).ToArray(),
            removeLicenses = removeSkuIds.ToArray()
        });
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Microsoft Graph retornou HTTP {(int)response.StatusCode} ao alterar licenças.");
    }

    private async Task<string> AcquireAccessTokenAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var tenantId = RequiredSetting("TenantId");
        var clientId = RequiredSetting("ClientId");
        var thumbprint = RequiredSetting("CertificateThumbprint");
        using var certificate = LoadCertificate(thumbprint);
        var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var header = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["alg"] = "RS256",
            ["typ"] = "JWT",
            ["x5t"] = Base64Url(certificate.GetCertHash())
        });
        var payload = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["aud"] = tokenUrl,
            ["iss"] = clientId,
            ["sub"] = clientId,
            ["jti"] = Guid.NewGuid().ToString(),
            ["nbf"] = now - 60,
            ["exp"] = now + 600
        });

        var unsigned = $"{Base64Url(Encoding.UTF8.GetBytes(header))}.{Base64Url(Encoding.UTF8.GetBytes(payload))}";
        using var rsa = certificate.GetRSAPrivateKey() ?? throw new InvalidOperationException("Chave privada RSA do certificado não está acessível.");
        var signature = rsa.SignData(Encoding.UTF8.GetBytes(unsigned), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var assertion = $"{unsigned}.{Base64Url(signature)}";

        using var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["scope"] = "https://graph.microsoft.com/.default",
            ["grant_type"] = "client_credentials",
            ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            ["client_assertion"] = assertion
        });
        using var response = await client.PostAsync(tokenUrl, body, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Falha na autenticação App-only do Microsoft Graph. HTTP {(int)response.StatusCode}.");

        using var document = JsonDocument.Parse(content);
        if (!document.RootElement.TryGetProperty("access_token", out var tokenElement))
            throw new InvalidOperationException("Microsoft Entra não retornou access_token.");

        return tokenElement.GetString() ?? throw new InvalidOperationException("Microsoft Entra retornou access_token vazio.");
    }

    private Task AuditAsync(
        string operationId,
        string chamado,
        string operatorName,
        string action,
        string status,
        string userPrincipalName,
        IEnumerable<string> attributes,
        IEnumerable<string> licenses,
        CancellationToken cancellationToken,
        string? errorType = null,
        int? errorCode = null) =>
        audit.AppendAsync(new ProvisioningAuditEntry
        {
            OperationId = operationId,
            Chamado = chamado,
            Operator = operatorName,
            TechnicalIdentity = TechnicalIdentity,
            Action = action,
            Status = status,
            UserPrincipalName = userPrincipalName,
            Attributes = attributes.ToList(),
            Licenses = licenses.ToList(),
            ErrorType = errorType,
            ErrorCode = errorCode
        }, cancellationToken);

    private string RequiredSetting(string key)
    {
        var value = Settings[key]?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Configuração obrigatória ausente: Automind:Microsoft365:{key}.");
        return value;
    }

    private static X509Certificate2 LoadCertificate(string thumbprint)
    {
        var normalized = thumbprint.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        var certificate = store.Certificates
            .Find(X509FindType.FindByThumbprint, normalized, validOnly: false)
            .OfType<X509Certificate2>()
            .FirstOrDefault(x => x.HasPrivateKey);

        if (certificate is null)
            throw new InvalidOperationException("Certificado do Microsoft Graph não encontrado em LocalMachine\\My ou sem chave privada acessível.");

        var now = DateTime.Now;
        if (now < certificate.NotBefore || now > certificate.NotAfter)
            throw new InvalidOperationException("Certificado do Microsoft Graph está fora do período de validade.");

        return certificate;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string uri, string token)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static StringContent JsonBody(object value) =>
        new(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");

    private static List<Guid> NormalizeSkuIds(IEnumerable<Guid>? skuIds) =>
        (skuIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();

    private static List<string> NamesFor(IEnumerable<Guid> ids, IReadOnlyDictionary<Guid, Microsoft365LicenseInfo> byId) =>
        ids.Select(id => byId.TryGetValue(id, out var license) ? license.DisplayName : id.ToString()).ToList();

    private static Microsoft365LicenseAssignmentStepResult Step(string key, bool success, string message) =>
        new() { Key = key, Success = success, Message = message };

    private static Microsoft365LicenseAssignmentResponse Failed(string? upn, string message) =>
        new() { Success = false, UserPrincipalName = upn, Message = message };

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string FriendlyFallback(string skuPartNumber)
    {
        if (string.IsNullOrWhiteSpace(skuPartNumber))
            return "Licença sem nome";

        return string.Join(' ', skuPartNumber
            .Replace('_', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private sealed record GraphUserSnapshot(
        string UserPrincipalName,
        string? UsageLocation,
        bool? OnPremisesSyncEnabled,
        IReadOnlySet<Guid> AssignedSkuIds,
        IReadOnlyList<GraphLicenseState> LicenseStates);

    private sealed record GraphLicenseState(Guid SkuId, string? State, string? Error);
}
