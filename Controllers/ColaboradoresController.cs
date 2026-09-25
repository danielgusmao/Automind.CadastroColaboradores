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
    IAdProvisioningWriteService adWriter,
    IAdAuthenticationService adAuthorization,
    IAccessSuggestionService access,
    ITopdeskRequestParser topdeskParser,
    IJobTitleTranslationService jobTitles,
    IMicrosoft365LicenseService microsoft365,
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

            vm.GruposSugeridos = (await TrySuggestAsync(vm.CargoIngles, vm.Departamento, vm.Login, vm.NomeCompleto, null, cancellationToken)).ToList();
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
            var groups = await access.SuggestAsync(request.Cargo, request.Departamento, request.Login, request.NomeCompleto, request.OuDistinguishedName, cancellationToken);
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
    public async Task<IActionResult> BuscarOutrosGrupos([FromBody] GroupSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var groups = await access.SearchGroupsAsync(request.Termo, 20, cancellationToken);
            return Json(new { success = true, groups });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha na busca manual de grupos do AD. Tipo: {ErrorType}; código: {Code}", exception.GetType().Name, exception.HResult);
            return Json(new { success = false, message = "Não foi possível pesquisar grupos no Active Directory." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidarAd([FromBody] AdProvisioningValidationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var context = await ValidateProvisioningAsync(request, cancellationToken);
            return Json(context.Response);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarUsuarioPiloto([FromBody] AdProvisioningCreateRequest request, CancellationToken cancellationToken)
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        Response.Headers["Pragma"] = "no-cache";

        if (!request.Confirmacao)
            return Json(CreateBlocked("A criação exige confirmação explícita do operador."));

        if (!adWriter.IsWriteModeEnabled)
            return Json(CreateBlocked("A aplicação permanece em modo ReadOnly. Nenhuma escrita foi executada."));

        var operatorName = User.Identity?.Name ?? HttpContext.Session.GetString("Usuario") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(operatorName) || !await adAuthorization.IsAuthorizedAsync(operatorName, cancellationToken))
            return Json(CreateBlocked("O operador não está autorizado pelo grupo configurado no Active Directory."));

        try
        {
            var context = await ValidateProvisioningAsync(request, cancellationToken);
            if (!context.Response.Valid || context.Response.Preview is null)
                return Json(CreateBlocked("A pré-validação não está íntegra. Nenhuma escrita foi executada."));

            if (!adWriter.WriteAllowedOuDns.Contains(context.Ou.DistinguishedName ?? string.Empty))
                return Json(CreateBlocked("A OU selecionada não pertence ao escopo de escrita do piloto."));

            var preview = context.Response.Preview;
            var command = new AdProvisioningWriteCommand
            {
                Chamado = (request.Chamado ?? string.Empty).Trim().ToUpperInvariant(),
                Operator = operatorName,
                Cn = preview.Cn,
                GivenName = preview.GivenName,
                Surname = preview.Surname,
                DisplayName = preview.Cn,
                Description = preview.Description ?? preview.Title ?? string.Empty,
                SamAccountName = preview.SamAccountName,
                UserPrincipalName = preview.UserPrincipalName,
                Mail = preview.Mail,
                Office = preview.Office,
                TelephoneNumber = preview.TelephoneNumber,
                Title = preview.Title,
                Department = preview.Department,
                Company = "Automind",
                ManagerDistinguishedName = preview.ManagerDistinguishedName ?? string.Empty,
                OuDistinguishedName = preview.OuDistinguishedName ?? string.Empty,
                GroupDns = context.RequestedGroups.ToList()
            };

            return Json(await adWriter.CreateUserAsync(command, cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha antes da chamada de escrita AD. Tipo: {ErrorType}; código: {Code}", exception.GetType().Name, exception.HResult);
            return Json(CreateBlocked("Não foi possível concluir as validações finais. Nenhuma nova escrita deve ser tentada até revisão."));
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AplicarMembershipPiloto([FromBody] AdPilotMembershipRequest request, CancellationToken cancellationToken)
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        Response.Headers["Pragma"] = "no-cache";

        if (!request.Confirmacao)
            return Json(new AdPilotMembershipResponse { Success = false, Message = "A membership exige confirmação explícita do operador." });

        var operatorName = User.Identity?.Name ?? HttpContext.Session.GetString("Usuario") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(operatorName) || !await adAuthorization.IsAuthorizedAsync(operatorName, cancellationToken))
            return Json(new AdPilotMembershipResponse { Success = false, Message = "O operador não está autorizado pelo grupo configurado no Active Directory." });

        return Json(await adWriter.ApplyPilotMembershipAsync("I2609-0295", operatorName, cancellationToken));
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
            GruposSugeridos = (await TrySuggestAsync("Automation Systems Analyst", "ENGENHARIA", "gabriel.silva", "Gabriel Luís Lima Silva", null, cancellationToken)).ToList()
        };
        await LoadOusAsync(cancellationToken);
        return View("Novo", vm);
    }

    private async Task<ProvisioningValidationContext> ValidateProvisioningAsync(
        AdProvisioningValidationRequest request,
        CancellationToken cancellationToken)
    {
        var suggestions = (await access.SuggestAsync(request.CargoIngles, request.Departamento, request.Login, request.NomeCompleto, request.OuDistinguishedName, cancellationToken)).ToList();

        var rawRequestedGroups = (request.SelectedGroupDns ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var resolvedRequestedGroups = (await access.ResolveGroupsAsync(rawRequestedGroups, cancellationToken)).ToList();
        var resolvedByDn = resolvedRequestedGroups
            .Where(x => !string.IsNullOrWhiteSpace(x.DistinguishedName))
            .ToDictionary(x => x.DistinguishedName, StringComparer.OrdinalIgnoreCase);

        var invalidRequestedGroups = rawRequestedGroups
            .Where(dn => !resolvedByDn.TryGetValue(dn, out var group) || group.Protegido)
            .ToList();

        var writeUnauthorizedGroups = adWriter.GroupWritesEnabled
            ? rawRequestedGroups.Where(dn => !adWriter.GroupWriteAllowedDns.Contains(dn)).ToList()
            : new List<string>();

        var requestedGroups = rawRequestedGroups
            .Where(dn => resolvedByDn.TryGetValue(dn, out var group) && !group.Protegido)
            .ToList();

        var identity = await ad.CheckIdentityAvailabilityAsync(request.Login ?? string.Empty, request.Email ?? string.Empty, cancellationToken);
        var manager = await ad.ResolveUserAsync(request.SuperiorImediato ?? string.Empty, cancellationToken);
        var ou = await ad.ValidateOrganizationalUnitAsync(request.OuDistinguishedName ?? string.Empty, cancellationToken);
        var commonName = ou.Exists && ou.Allowed
            ? await ad.CheckCommonNameAvailabilityAsync(request.NomeCompleto ?? string.Empty, ou.DistinguishedName ?? string.Empty, cancellationToken)
            : new AdCommonNameAvailability { Available = false };
        var groups = await ad.ValidateGroupsAsync(requestedGroups, cancellationToken);
        var samValidation = ValidateSamAccountName(request.Login);
        var ticketValidation = ValidateTicket(request.Chamado);
        var nameValidation = ValidateFullName(request.NomeCompleto);
        var emailValidation = ValidateCorporateEmail(request.Login, request.Email);
        var organizationValidation = ValidateOrganization(request.CargoIngles, request.Departamento);

        foreach (var suggestion in suggestions)
            suggestion.Selecionado = !suggestion.Protegido && requestedGroups.Contains(suggestion.DistinguishedName, StringComparer.OrdinalIgnoreCase);

        var groupsPassed = invalidRequestedGroups.Count == 0
            && writeUnauthorizedGroups.Count == 0
            && groups.AllExist;
        var checks = new List<AdValidationCheck>
        {
            Check("ticket", "Chamado TOPdesk válido", ticketValidation.Valid, ticketValidation.Message),
            Check("name", "Nome completo válido", nameValidation.Valid, nameValidation.Message),
            Check("login", "Login válido e disponível", samValidation.Valid && identity.LoginAvailable,
                !samValidation.Valid ? samValidation.Message : identity.LoginAvailable ? "sAMAccountName válido e livre." : CollisionMessage(identity)),
            Check("cn", "CN disponível na OU", commonName.Available,
                commonName.Available ? "Nome do objeto livre na OU selecionada." : CommonNameMessage(commonName, request.NomeCompleto)),
            Check("upn", "UPN disponível", emailValidation.Valid && identity.UpnAvailable,
                !emailValidation.Valid ? emailValidation.Message : identity.UpnAvailable ? "UPN corporativo livre." : CollisionMessage(identity)),
            Check("email", "E-mail / SMTP disponível", emailValidation.Valid && identity.EmailAvailable,
                !emailValidation.Valid ? emailValidation.Message : identity.EmailAvailable ? "mail e proxyAddresses livres para os domínios configurados." : CollisionMessage(identity)),
            Check("organization", "Cargo e departamento preenchidos", organizationValidation.Valid, organizationValidation.Message),
            Check("manager", "Superior localizado", manager.Found && !manager.Ambiguous, ManagerMessage(manager)),
            Check("ou", "OU válida", ou.Exists && ou.Allowed,
                ou.Exists && ou.Allowed ? $"OU confirmada: {ou.DisplayName}." : "A OU não existe no AD ou não está na lista permitida."),
            Check("groups", "Grupos válidos", groupsPassed, GroupMessage(requestedGroups, groups, invalidRequestedGroups, writeUnauthorizedGroups, adWriter.GroupWritesEnabled))
        };

        var valid = checks.All(x => x.Passed);
        var preview = valid ? BuildPreview(request, manager, ou, resolvedRequestedGroups, requestedGroups) : null;
        var response = new AdProvisioningValidationResponse
        {
            Success = true,
            Valid = valid,
            Message = valid
                ? "Pré-validação concluída no Active Directory. Nenhuma escrita foi executada."
                : "A pré-validação encontrou pendências. Nenhuma escrita foi executada.",
            Checks = checks,
            Groups = suggestions,
            Preview = preview
        };

        return new ProvisioningValidationContext(response, manager, ou, requestedGroups);
    }

    private async Task LoadOusAsync(CancellationToken cancellationToken)
    {
        ViewBag.AdWriteMode = adWriter.IsWriteModeEnabled;
        ViewBag.AdMode = adWriter.Mode;
        ViewBag.AdGroupWritesEnabled = adWriter.GroupWritesEnabled;
        ViewBag.PilotMembershipTestEnabled = adWriter.PilotMembershipTestEnabled;
        ViewBag.PilotMembershipUserDn = adWriter.PilotMembershipUserDn;
        ViewBag.PilotMembershipGroupDn = adWriter.PilotMembershipGroupDn;
        ViewBag.WriteAllowedOuDns = adWriter.WriteAllowedOuDns.ToArray();
        ViewBag.GroupWriteAllowedDns = adWriter.GroupWriteAllowedDns.ToArray();
        ViewBag.Microsoft365Enabled = microsoft365.IsEnabled;
        ViewBag.Microsoft365LicenseWritesEnabled = microsoft365.LicenseWritesEnabled;

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

        if (!microsoft365.IsEnabled)
        {
            ViewBag.Microsoft365Licenses = Array.Empty<Microsoft365LicenseInfo>();
            ViewBag.Microsoft365Error = null;
            return;
        }

        try
        {
            ViewBag.Microsoft365Licenses = await microsoft365.GetSubscribedLicensesAsync(cancellationToken);
            ViewBag.Microsoft365Error = null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha ao consultar licenças do Microsoft 365. Tipo: {ErrorType}; código: {Code}", exception.GetType().Name, exception.HResult);
            ViewBag.Microsoft365Licenses = Array.Empty<Microsoft365LicenseInfo>();
            ViewBag.Microsoft365Error = "Não foi possível consultar as licenças no Microsoft 365. O restante do formulário continua disponível.";
        }
    }

    private async Task<IReadOnlyList<GroupSuggestion>> TrySuggestAsync(
        string? cargo,
        string? departamento,
        string? excludedSamAccountName,
        string? excludedCommonName,
        string? excludedOuDistinguishedName,
        CancellationToken cancellationToken)
    {
        try
        {
            return await access.SuggestAsync(cargo, departamento, excludedSamAccountName, excludedCommonName, excludedOuDistinguishedName, cancellationToken);
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
        IReadOnlyList<GroupSuggestion> selectedGroupMetadata,
        IReadOnlyCollection<string> requestedGroups)
    {
        var fullName = (request.NomeCompleto ?? string.Empty).Trim();
        var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var givenName = nameParts.FirstOrDefault() ?? string.Empty;
        var surname = nameParts.Length > 1 ? string.Join(' ', nameParts.Skip(1)) : string.Empty;
        var login = (request.Login ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();

        var groupNames = selectedGroupMetadata
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
            Description = request.CargoIngles?.Trim(),
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

    private static (bool Valid, string Message) ValidateTicket(string? value)
    {
        var ticket = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(ticket)) return (false, "Informe o chamado TOPdesk.");
        return Regex.IsMatch(ticket, @"^I\d{4}-\d{4,6}$", RegexOptions.CultureInvariant)
            ? (true, "Chamado TOPdesk informado.")
            : (false, "Formato de chamado TOPdesk inválido.");
    }

    private static (bool Valid, string Message) ValidateFullName(string? value)
    {
        var name = (value ?? string.Empty).Trim();
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return (false, "Informe nome e sobrenome.");
        if (name.Length > 64) return (false, "O CN excede 64 caracteres e precisa ser ajustado antes da criação.");
        return (true, "Nome completo válido para CN/givenName/sn.");
    }

    private (bool Valid, string Message) ValidateCorporateEmail(string? loginValue, string? emailValue)
    {
        var login = (loginValue ?? string.Empty).Trim();
        var email = (emailValue ?? string.Empty).Trim();
        var expected = string.IsNullOrWhiteSpace(login) ? string.Empty : $"{login}@{directory.EmailDomain}";
        return !string.IsNullOrWhiteSpace(expected) && string.Equals(email, expected, StringComparison.OrdinalIgnoreCase)
            ? (true, $"E-mail corporativo no domínio {directory.EmailDomain}.")
            : (false, $"O e-mail deve ser exatamente {expected}.");
    }

    private static (bool Valid, string Message) ValidateOrganization(string? title, string? department)
    {
        if (string.IsNullOrWhiteSpace(title)) return (false, "Informe o cargo em inglês.");
        if (string.IsNullOrWhiteSpace(department)) return (false, "Informe o departamento.");
        return (true, "Cargo e departamento preenchidos.");
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

    private static string GroupMessage(
        IReadOnlyCollection<string> requestedGroups,
        AdGroupValidation result,
        IReadOnlyCollection<string> invalidRequestedGroups,
        IReadOnlyCollection<string> writeUnauthorizedGroups,
        bool groupWritesEnabled)
    {
        if (invalidRequestedGroups.Count > 0)
            return $"Há grupo(s) não localizado(s) ou protegido(s): {string.Join("; ", invalidRequestedGroups)}";
        if (writeUnauthorizedGroups.Count > 0)
            return $"Há grupo(s) válido(s), porém fora da allowlist de escrita do piloto: {string.Join("; ", writeUnauthorizedGroups)}";
        if (requestedGroups.Count == 0) return "Nenhum grupo selecionado; o piloto pode seguir sem memberships.";
        if (result.AllExist)
            return groupWritesEnabled
                ? $"{requestedGroups.Count} grupo(s) confirmado(s) e autorizado(s) para escrita no piloto."
                : $"{requestedGroups.Count} grupo(s) confirmado(s) no AD; memberships permanecem bloqueadas.";
        return $"Grupos não localizados: {string.Join("; ", result.MissingGroups)}";
    }

    private static AdProvisioningCreateResponse CreateBlocked(string message)
        => new()
        {
            Success = false,
            UserCreated = false,
            Enabled = false,
            RequiresManualReview = false,
            Message = message
        };

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

    private sealed record ProvisioningValidationContext(
        AdProvisioningValidationResponse Response,
        AdUserResolution Manager,
        AdOuValidation Ou,
        IReadOnlyList<string> RequestedGroups);

    public sealed class GroupSuggestionRequest
    {
        public string? Cargo { get; set; }
        public string? Departamento { get; set; }
        public string? Login { get; set; }
        public string? NomeCompleto { get; set; }
        public string? OuDistinguishedName { get; set; }
    }

    public sealed class GroupSearchRequest
    {
        public string? Termo { get; set; }
    }
}
