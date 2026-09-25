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
    ILogger<Microsoft365LicenseService> logger) : IMicrosoft365LicenseService
{
    private const string CacheKey = "m365:subscribed-skus";

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

    public bool IsEnabled => Settings.GetValue<bool>("Enabled") && Settings.GetValue<bool>("LicenseInventoryEnabled");
    public bool LicenseWritesEnabled => Settings.GetValue<bool>("LicenseWritesEnabled");

    public async Task<IReadOnlyList<Microsoft365LicenseInfo>> GetSubscribedLicensesAsync(CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
            return [];

        if (cache.TryGetValue(CacheKey, out IReadOnlyList<Microsoft365LicenseInfo>? cached) && cached is not null)
            return cached;

        var tenantId = RequiredSetting("TenantId");
        var clientId = RequiredSetting("ClientId");
        var thumbprint = RequiredSetting("CertificateThumbprint");
        using var certificate = LoadCertificate(thumbprint);
        var client = httpClientFactory.CreateClient("MicrosoftGraph");
        var token = await AcquireAccessTokenAsync(client, certificate, tenantId, clientId, cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get,
            "https://graph.microsoft.com/v1.0/subscribedSkus?$select=skuId,skuPartNumber,consumedUnits,prepaidUnits,capabilityStatus");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

    private static async Task<string> AcquireAccessTokenAsync(
        HttpClient client,
        X509Certificate2 certificate,
        string tenantId,
        string clientId,
        CancellationToken cancellationToken)
    {
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
}
