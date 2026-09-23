using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Automind.CadastroColaboradores.Models;
using Automind.CadastroColaboradores.Services;
using Microsoft.AspNetCore.Mvc;

namespace Automind.CadastroColaboradores.Controllers;

public sealed class ColaboradoresController(
    IAdReadOnlyService ad,
    IAccessSuggestionService access,
    ITopdeskRequestParser topdeskParser,
    IJobTitleTranslationService jobTitles,
    AdConnectionFactory directory,
    ILogger<ColaboradoresController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Novo(CancellationToken cancellationToken)
    {
        await LoadOusAsync(cancellationToken);
        return View(new CollaboratorFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportarTopdesk(string incidentJson, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(incidentJson))
        {
            TempData["TopdeskError"] = "O TOPdesk não retornou dados do chamado.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            using var document = JsonDocument.Parse(incidentJson);
            var root = document.RootElement;

            var numero = GetString(root, "number");
            var request = GetString(root, "request");
            var descricao = GetString(root, "briefDescription");

            if (string.IsNullOrWhiteSpace(numero) || string.IsNullOrWhiteSpace(request))
            {
                TempData["TopdeskError"] = "O chamado foi localizado, mas não contém os dados necessários para importação.";
                return RedirectToAction("Index", "Home");
            }

            if (!string.Equals(descricao?.Trim(), "CRIAÇÃO DE USUÁRIO", StringComparison.OrdinalIgnoreCase))
            {
                TempData["TopdeskError"] = $"O chamado {numero} não é do tipo CRIAÇÃO DE USUÁRIO.";
                return RedirectToAction("Index", "Home");
            }

            var dados = topdeskParser.Parse(request);
            var observacao = dados.Get("Observação") ?? string.Empty;

            var nomeCompleto = NormalizarNome(dados.Get("Nome Completo"));
            var nomeGuerra = NormalizarNomeGuerra(dados.Get("Nome de Guerra"));
            var login = GerarLogin(nomeGuerra);

            var cargoPortugues =
                ExtrairValorObservacao(observacao, "Nome do cargo")
                ?? dados.Get("Descrição do cargo");

            var cargoIngles =
                ExtrairValorObservacao(observacao, "Em Inglês")
                ?? ExtrairValorObservacao(observacao, "Em Ingles")
                ?? jobTitles.Translate(cargoPortugues);

            var grupoTrabalho = dados.Get("Grupo de Trabalho")?.Trim();
            var departamento = grupoTrabalho?.ToUpper(new CultureInfo("pt-BR"));

            var vm = new CollaboratorFormViewModel
            {
                Chamado = numero.ToUpperInvariant(),
                Fonte = "TOPdesk",
                NomeCompleto = nomeCompleto,
                NomeGuerra = nomeGuerra,
                Login = login,
                Email = string.IsNullOrWhiteSpace(login) ? string.Empty : $"{login}@{directory.EmailDomain}",
                TelefoneCelular = dados.Get("Telefone Celular"),
                DivulgarContato = EhSim(dados.Get("Deseja divulgar contato na intranet")),
                LocalTrabalho = dados.Get("Local de Trabalho")?.Trim(),
                SuperiorImediato = LimparSuperior(dados.Get("Superior Imediato")),
                CargoPortugues = cargoPortugues?.Trim(),
                CargoIngles = cargoIngles?.Trim(),
                Departamento = departamento,
                GrupoTrabalho = grupoTrabalho,
                PerfilUsuario = dados.Get("Perfil de usuário")?.Trim()
            };

            vm.GruposSugeridos = (await TrySuggestAsync(vm.CargoIngles, vm.Departamento, cancellationToken)).ToList();
            await LoadOusAsync(cancellationToken);
            return View("Novo", vm);
        }
        catch (JsonException)
        {
            TempData["TopdeskError"] = "A extensão retornou um JSON inválido do TOPdesk.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuscarGrupos([FromBody] GroupSuggestionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var groups = await access.SuggestAsync(request.Cargo, request.Departamento, cancellationToken);
            return Json(new { success = true, groups });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha na consulta de grupos do AD. Tipo: {ErrorType}; código: {Code}", exception.GetType().Name, exception.HResult);
            return Json(new { success = false, message = "Não foi possível consultar os grupos no Active Directory." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidarAd([FromBody] AdProvisioningValidationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var suggestions = (await access.SuggestAsync(request.CargoIngles, request.Departamento, cancellationToken)).ToList();
            var selectableDns = suggestions
                .Where(x => !x.Protegido)
                .Select(x => x.DistinguishedName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var requestedGroups = request.SelectedGroupDns
                .Where(x => !string.IsNullOrWhiteSpace(x) && selectableDns.Contains(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var identity = await ad.CheckIdentityAvailabilityAsync(request.Login ?? string.Empty, request.Email ?? string.Empty, cancellationToken);
            var manager = await ad.ResolveUserAsync(request.SuperiorImediato ?? string.Empty, cancellationToken);
            var ou = await ad.ValidateOrganizationalUnitAsync(request.OuDistinguishedName ?? string.Empty, cancellationToken);
            var commonName = ou.Exists && ou.Allowed
                ? await ad.CheckCommonNameAvailabilityAsync(request.NomeCompleto ?? string.Empty, ou.DistinguishedName ?? string.Empty, cancellationToken)
                : new AdCommonNameAvailability { Available = false };
            var groups = await ad.ValidateGroupsAsync(requestedGroups, cancellationToken);
            var samValidation = ValidateSamAccountName(request.Login);

            foreach (var suggestion in suggestions)
            {
                suggestion.Selecionado = !suggestion.Protegido && requestedGroups.Contains(suggestion.DistinguishedName, StringComparer.OrdinalIgnoreCase);
            }

            var checks = new List<AdValidationCheck>
            {
                Check("login", "Login válido e disponível", samValidation.Valid && identity.LoginAvailable,
                    !samValidation.Valid ? samValidation.Message : identity.LoginAvailable ? "sAMAccountName válido e livre." : CollisionMessage(identity)),
                Check("cn", "CN disponível na OU", commonName.Available,
                    commonName.Available ? "Nome do objeto livre na OU selecionada." : CommonNameMessage(commonName, request.NomeCompleto)),
                Check("upn", "UPN disponível", identity.UpnAvailable,
                    identity.UpnAvailable ? "UPN livre." : CollisionMessage(identity)),
                Check("email", "E-mail / SMTP disponível", identity.EmailAvailable,
                    identity.EmailAvailable ? "mail e proxyAddresses livres para os domínios configurados." : CollisionMessage(identity)),
                Check("manager", "Superior localizado", manager.Found && !manager.Ambiguous,
                    ManagerMessage(manager)),
                Check("ou", "OU válida", ou.Exists && ou.Allowed,
                    ou.Exists && ou.Allowed ? $"OU confirmada: {ou.DisplayName}." : "A OU não existe no AD ou não está na lista permitida."),
                Check("groups", "Grupos existentes", requestedGroups.Count > 0 && groups.AllExist,
                    GroupMessage(requestedGroups, groups))
            };

            var valid = checks.All(x => x.Passed);
            var preview = valid
                ? BuildPreview(request, manager, ou, suggestions, requestedGroups)
                : null;

            return Json(new AdProvisioningValidationResponse
            {
                Success = true,
                Valid = valid,
                Message = valid
                    ? "Pré-validação concluída no Active Directory. Nenhuma escrita foi executada."
                    : "A pré-validação encontrou pendências. Nenhuma escrita foi executada.",
                Checks = checks,
                Groups = suggestions,
                Preview = preview
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha na pré-validação AD. Tipo: {ErrorType}; código: {Code}", exception.GetType().Name, exception.HResult);
            return Json(new AdProvisioningValidationResponse
            {
                Success = false,
                Valid = false,
                Message = "Não foi possível concluir a consulta no Active Directory. Nenhuma alteração foi realizada."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExemploTopdesk(CancellationToken cancellationToken)
    {
        var vm = new CollaboratorFormViewModel
        {
            Chamado = "I2609-0223",
            Fonte = "TOPdesk",
            NomeCompleto = NormalizarNome("GABRIEL LUÍS LIMA SILVA"),
            NomeGuerra = "Gabriel Silva",
            Login = "gabriel.silva",
            Email = $"gabriel.silva@{directory.EmailDomain}",
            TelefoneCelular = "7181696721",
            DivulgarContato = true,
            LocalTrabalho = "Salvador - Sede",
            SuperiorImediato = "Edson Neto",
            CargoPortugues = "Analista de Sistemas de Automação",
            CargoIngles = "Automation Systems Analyst",
            Departamento = "ENGENHARIA",
            GrupoTrabalho = "Engenharia",
            PerfilUsuario = "Colaborador Interno",
            GruposSugeridos = (await TrySuggestAsync("Automation Systems Analyst", "ENGENHARIA", cancellationToken)).ToList()
        };
        await LoadOusAsync(cancellationToken);
        return View("Novo", vm);
    }

    private async Task LoadOusAsync(CancellationToken cancellationToken)
    {
        try
        {
            ViewBag.Ous = await ad.GetOrganizationalUnitsAsync(cancellationToken);
            ViewBag.AdReadError = null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha ao listar OUs do AD. Tipo: {ErrorType}; código: {Code}", exception.GetType().Name, exception.HResult);
            ViewBag.Ous = Array.Empty<OrganizationalUnitOption>();
            ViewBag.AdReadError = "Não foi possível listar as OUs no Active Directory. Verifique a identidade do pool e a conectividade do servidor.";
        }
    }

    private async Task<IReadOnlyList<GroupSuggestion>> TrySuggestAsync(string? cargo, string? departamento, CancellationToken cancellationToken)
    {
        try
        {
            return await access.SuggestAsync(cargo, departamento, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha ao sugerir grupos do AD. Tipo: {ErrorType}; código: {Code}", exception.GetType().Name, exception.HResult);
            ViewBag.AdGroupError = "Os dados do TOPdesk foram importados, mas a sugestão de grupos do Active Directory não pôde ser carregada.";
            return [];
        }
    }

    private AdProvisioningPreview BuildPreview(
        AdProvisioningValidationRequest request,
        AdUserResolution manager,
        AdOuValidation ou,
        IReadOnlyList<GroupSuggestion> suggestions,
        IReadOnlyCollection<string> requestedGroups)
    {
        var fullName = (request.NomeCompleto ?? string.Empty).Trim();
        var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var givenName = nameParts.FirstOrDefault() ?? string.Empty;
        var surname = nameParts.Length > 1 ? string.Join(' ', nameParts.Skip(1)) : string.Empty;
        var login = (request.Login ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();

        var groupNames = suggestions
            .Where(x => requestedGroups.Contains(x.DistinguishedName, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Nome)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new AdProvisioningPreview
        {
            Cn = fullName,
            GivenName = givenName,
            Surname = surname,
            SamAccountName = login,
            UserPrincipalName = email,
            Mail = email,
            PrimarySmtp = string.IsNullOrWhiteSpace(login) ? string.Empty : $"SMTP:{login}@{directory.PrimarySmtpDomain}",
            SecondarySmtp = string.IsNullOrWhiteSpace(email) ? string.Empty : $"smtp:{email}",
            Company = "Automind",
            Title = request.CargoIngles?.Trim(),
            Department = request.Departamento?.Trim(),
            Office = request.LocalTrabalho?.Trim(),
            TelephoneNumber = request.DivulgarContato ? request.TelefoneCelular?.Trim() : null,
            ManagerDistinguishedName = manager.DistinguishedName,
            OuDistinguishedName = ou.DistinguishedName,
            Groups = groupNames
        };
    }

    private static (bool Valid, string Message) ValidateSamAccountName(string? value)
    {
        var login = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login)) return (false, "Informe o sAMAccountName.");
        if (login.Length > 20) return (false, $"sAMAccountName possui {login.Length} caracteres; o limite para usuários é 20.");
        if (login.IndexOfAny(new[] { '"', '/', '\\', '[', ']', ':', ';', '|', '=', ',', '+', '*', '?', '<', '>' }) >= 0) return (false, "sAMAccountName contém caractere não permitido pelo Active Directory.");
        return (true, "sAMAccountName dentro das restrições de formato.");
    }

    private static string CommonNameMessage(AdCommonNameAvailability result, string? commonName)
        => !string.IsNullOrWhiteSpace(result.ExistingDistinguishedName)
            ? $"Já existe um objeto com CN '{commonName?.Trim()}' na OU selecionada: {result.ExistingDistinguishedName}"
            : "Não foi possível confirmar a disponibilidade do CN na OU selecionada.";

    private static AdValidationCheck Check(string key, string label, bool passed, string message)
        => new() { Key = key, Label = label, Passed = passed, Message = message };

    private static string CollisionMessage(AdIdentityAvailability identity)
        => identity.Collisions.Count == 0
            ? "Foi encontrada uma colisão de identidade."
            : $"Colisão encontrada em: {string.Join(" | ", identity.Collisions.Take(3))}";

    private static string ManagerMessage(AdUserResolution manager)
    {
        if (manager.Ambiguous) return $"Foram encontrados {manager.Matches} usuários com esse identificador.";
        if (!manager.Found) return "Superior não localizado entre usuários ativos.";
        return $"Superior confirmado: {manager.DisplayName} ({manager.SamAccountName}).";
    }

    private static string GroupMessage(IReadOnlyCollection<string> requestedGroups, AdGroupValidation result)
    {
        if (requestedGroups.Count == 0) return "Nenhum grupo selecionado para validação.";
        if (result.AllExist) return $"{requestedGroups.Count} grupo(s) confirmado(s) no AD.";
        return $"Grupos não localizados: {string.Join("; ", result.MissingGroups)}";
    }

    private static string? GetString(JsonElement root, string property)
    {
        return root.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : null;
    }

    private static bool EhSim(string? value)
        => string.Equals(value?.Trim(), "Sim", StringComparison.OrdinalIgnoreCase);

    private static string LimparSuperior(string? value)
        => (value ?? string.Empty).Trim().TrimEnd('.', ',', ';', ':');

    private static string NormalizarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome)) return string.Empty;

        var culture = new CultureInfo("pt-BR");
        var textInfo = culture.TextInfo;
        var lower = nome.Trim().ToLower(culture);
        var result = textInfo.ToTitleCase(lower);

        foreach (var p in new[] { " Da ", " De ", " Do ", " Das ", " Dos " })
            result = result.Replace(p, p.ToLowerInvariant());

        return result;
    }

    private static string NormalizarNomeGuerra(string? nome)
    {
        var normalizado = NormalizarNome(nome);
        if (string.IsNullOrWhiteSpace(normalizado)) return string.Empty;

        var partes = normalizado
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !EhParticula(x))
            .ToArray();

        return partes.Length switch
        {
            0 => string.Empty,
            1 => partes[0],
            _ => $"{partes[0]} {partes[^1]}"
        };
    }

    private static bool EhParticula(string value)
        => value.Equals("da", StringComparison.OrdinalIgnoreCase)
        || value.Equals("de", StringComparison.OrdinalIgnoreCase)
        || value.Equals("do", StringComparison.OrdinalIgnoreCase)
        || value.Equals("das", StringComparison.OrdinalIgnoreCase)
        || value.Equals("dos", StringComparison.OrdinalIgnoreCase);

    private static string GerarLogin(string nomeGuerra)
    {
        if (string.IsNullOrWhiteSpace(nomeGuerra)) return string.Empty;

        var semAcentos = RemoverAcentos(nomeGuerra).ToLowerInvariant();
        var partes = Regex.Split(semAcentos.Trim(), @"[\s.]+")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => Regex.Replace(x, @"[^a-z0-9]", string.Empty))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        return string.Join('.', partes);
    }

    private static string RemoverAcentos(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string? ExtrairValorObservacao(string observacao, string chave)
    {
        if (string.IsNullOrWhiteSpace(observacao)) return null;

        var regex = new Regex(
            $@"(?im)^\s*{Regex.Escape(chave)}\s*:\s*(.+?)\s*$",
            RegexOptions.CultureInvariant);

        var match = regex.Match(observacao);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    public sealed class GroupSuggestionRequest
    {
        public string? Cargo { get; set; }
        public string? Departamento { get; set; }
    }
}
