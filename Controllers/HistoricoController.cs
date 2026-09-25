using System.Text.RegularExpressions;
using Automind.CadastroColaboradores.Models;
using Automind.CadastroColaboradores.Services;
using Microsoft.AspNetCore.Mvc;

namespace Automind.CadastroColaboradores.Controllers;

public sealed class HistoricoController(
    IProvisioningAuditService audit,
    IAdReadOnlyService ad,
    IAdProvisioningWriteService adWriter,
    IAdAuthenticationService adAuthorization,
    IMicrosoft365LicenseService microsoft365,
    AdConnectionFactory directory,
    ILogger<HistoricoController> logger) : Controller
{
    private static readonly Regex TicketRegex = new("^I\\d{4}-\\d{4}$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var entries = await audit.ReadRecentAsync(2000, cancellationToken);
        var grouped = entries
            .Where(x => !string.IsNullOrWhiteSpace(x.Chamado))
            .GroupBy(x => x.Chamado.Trim().ToUpperInvariant(), StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Max(x => x.TimestampUtc))
            .Take(40)
            .ToList();

        IReadOnlyList<Microsoft365LicenseInfo> inventory = [];
        try
        {
            if (microsoft365.IsEnabled)
                inventory = await microsoft365.GetSubscribedLicensesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning("Falha ao carregar inventario M365 no historico. Tipo: {ErrorType}; codigo: {Code}", exception.GetType().Name, exception.HResult);
        }

        var items = new List<ProvisioningHistoryItem>();
        foreach (var group in grouped)
        {
            var ordered = group.OrderBy(x => x.TimestampUtc).ToList();
            var latest = ordered[^1];
            var distinguishedName = ordered.Select(x => x.DistinguishedName).LastOrDefault(x => !string.IsNullOrWhiteSpace(x));
            var upn = ordered.Select(x => x.UserPrincipalName).LastOrDefault(x => !string.IsNullOrWhiteSpace(x));
            var displayName = ExtractCn(distinguishedName);

            if (string.IsNullOrWhiteSpace(upn) && !string.IsNullOrWhiteSpace(displayName))
            {
                try
                {
                    var resolved = await ad.ResolveUserAsync(displayName, cancellationToken);
                    if (resolved.Found && !resolved.Ambiguous)
                    {
                        upn = resolved.UserPrincipalName;
                        displayName = resolved.DisplayName ?? displayName;
                        distinguishedName ??= resolved.DistinguishedName;
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogWarning("Falha ao resolver usuario do historico {Chamado}. Tipo: {ErrorType}; codigo: {Code}", group.Key, exception.GetType().Name, exception.HResult);
                }
            }

            var completed = ordered.Any(x => x.Action == "provisioning-complete" && x.Status == "success");
            var failed = ordered.Any(x => x.Action == "provisioning-failed" && x.Status == "failed") && !completed;
            var pendingEntry = ordered.LastOrDefault(x => x.Action == "m365-license-pending");
            var pendingIds = (pendingEntry?.Licenses ?? [])
                .Select(ParseSkuId)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var pendingNames = pendingIds
                .Select(id => inventory.FirstOrDefault(x => x.SkuId == id)?.DisplayName ?? id.ToString())
                .ToList();

            items.Add(new ProvisioningHistoryItem
            {
                Chamado = group.Key,
                LastEventUtc = latest.TimestampUtc,
                Operator = latest.Operator,
                DisplayName = displayName ?? upn ?? distinguishedName ?? "Usuario",
                UserPrincipalName = upn,
                DistinguishedName = distinguishedName,
                AdCompleted = completed,
                AdFailed = failed,
                Groups = ordered.SelectMany(x => x.Groups ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                PendingLicenseSkuIds = pendingIds,
                PendingLicenseNames = pendingNames
            });
        }

        var upns = items
            .Where(x => x.AdCompleted && !x.AdFailed && !string.IsNullOrWhiteSpace(x.UserPrincipalName))
            .Select(x => x.UserPrincipalName!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        IReadOnlyDictionary<string, Microsoft365UserLicenseStatus> statuses = new Dictionary<string, Microsoft365UserLicenseStatus>(StringComparer.OrdinalIgnoreCase);
        if (upns.Length > 0 && microsoft365.IsEnabled)
        {
            try
            {
                statuses = await microsoft365.GetUserLicenseStatusesAsync(upns, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning("Falha ao consultar status M365 do historico. Tipo: {ErrorType}; codigo: {Code}", exception.GetType().Name, exception.HResult);
            }
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.UserPrincipalName))
                continue;

            if (!statuses.TryGetValue(item.UserPrincipalName, out var status))
            {
                item.Microsoft365Checked = false;
                continue;
            }

            item.Microsoft365Checked = true;
            item.EntraExists = status.Exists;
            item.UsageLocation = status.UsageLocation;
            item.Microsoft365Error = status.ErrorMessage;
            item.AssignedLicenses = status.AssignedLicenses;
        }

        return View(new ProvisioningHistoryViewModel
        {
            Items = items,
            AssignableLicenses = inventory.Where(x => x.CanAssign).OrderBy(x => x.DisplayName).ToArray()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtribuirLicenca(string chamado, string userPrincipalName, Guid skuId, bool confirmacao, CancellationToken cancellationToken)
    {
        if (!confirmacao)
        {
            TempData["HistoryError"] = "A atribuicao exige confirmacao explicita.";
            return RedirectToAction(nameof(Index));
        }

        var ticket = (chamado ?? string.Empty).Trim().ToUpperInvariant();
        var upn = (userPrincipalName ?? string.Empty).Trim();
        if (!TicketRegex.IsMatch(ticket) || skuId == Guid.Empty)
        {
            TempData["HistoryError"] = "Chamado ou licenca invalida.";
            return RedirectToAction(nameof(Index));
        }

        var operatorName = User.Identity?.Name ?? HttpContext.Session.GetString("Usuario") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(operatorName) || !await adAuthorization.IsAuthorizedAsync(operatorName, cancellationToken))
        {
            TempData["HistoryError"] = "Operador nao autorizado.";
            return RedirectToAction(nameof(Index));
        }

        if (!microsoft365.LicenseWritesEnabled)
        {
            TempData["HistoryError"] = "A escrita de licencas Microsoft 365 esta desabilitada.";
            return RedirectToAction(nameof(Index));
        }

        if (!upn.EndsWith($"@{directory.EmailDomain}", StringComparison.OrdinalIgnoreCase))
        {
            TempData["HistoryError"] = "UPN fora do dominio corporativo autorizado.";
            return RedirectToAction(nameof(Index));
        }

        var atIndex = upn.IndexOf('@');
        var samAccountName = atIndex > 0 ? upn[..atIndex] : upn;
        var adUser = await ad.ResolveUserAsync(samAccountName, cancellationToken);
        if (!adUser.Found || adUser.Ambiguous || string.IsNullOrWhiteSpace(adUser.DistinguishedName) ||
            !string.Equals(adUser.UserPrincipalName, upn, StringComparison.OrdinalIgnoreCase))
        {
            TempData["HistoryError"] = "Usuario nao localizado de forma unica no AD ou UPN divergente.";
            return RedirectToAction(nameof(Index));
        }

        if (!adWriter.WriteAllowedOuDns.Any(ou => adUser.DistinguishedName.EndsWith($",{ou}", StringComparison.OrdinalIgnoreCase)))
        {
            TempData["HistoryError"] = "Usuario fora do escopo de escrita piloto.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var validation = await microsoft365.ValidateSelectionAsync([skuId], cancellationToken);
            if (!validation.Valid)
            {
                TempData["HistoryError"] = validation.Message;
                return RedirectToAction(nameof(Index));
            }

            var result = await microsoft365.AssignLicensesAsync(ticket, operatorName, upn, [skuId], cancellationToken);
            if (result.Success)
                TempData["HistorySuccess"] = result.Message;
            else if (result.PendingSynchronization)
                TempData["HistoryWarning"] = result.Message;
            else
                TempData["HistoryError"] = result.Message;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha na recuperacao M365 pelo historico para {UserPrincipalName}. Tipo: {ErrorType}; codigo: {Code}", upn, exception.GetType().Name, exception.HResult);
            TempData["HistoryError"] = "Não foi possível concluir a atribuição pelo Histórico. Nenhuma nova tentativa automática foi feita.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static Guid ParseSkuId(string? value)
    {
        if (Guid.TryParse(value, out var id)) return id;
        if (!string.IsNullOrWhiteSpace(value))
        {
            var pipe = value.LastIndexOf('|');
            if (pipe >= 0 && Guid.TryParse(value[(pipe + 1)..], out id)) return id;
        }
        return Guid.Empty;
    }

    private static string? ExtractCn(string? distinguishedName)
    {
        if (string.IsNullOrWhiteSpace(distinguishedName)) return null;
        var text = distinguishedName.Trim();
        if (!text.StartsWith("CN=", StringComparison.OrdinalIgnoreCase)) return null;
        var comma = text.IndexOf(',');
        return comma > 3 ? text[3..comma].Replace("\\,", ",", StringComparison.Ordinal) : null;
    }
}
