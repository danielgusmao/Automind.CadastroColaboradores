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
    ITopdeskRequestParser topdeskParser) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Novo(CancellationToken cancellationToken)
    {
        ViewBag.Ous = await ad.GetOrganizationalUnitsAsync(cancellationToken);
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
                ?? ExtrairValorObservacao(observacao, "Em Ingles");

            var grupoTrabalho = dados.Get("Grupo de Trabalho")?.Trim();
            var departamento = grupoTrabalho?.ToUpper(new CultureInfo("pt-BR"));

            var vm = new CollaboratorFormViewModel
            {
                Chamado = numero.ToUpperInvariant(),
                Fonte = "TOPdesk",
                NomeCompleto = nomeCompleto,
                NomeGuerra = nomeGuerra,
                Login = login,
                Email = string.IsNullOrWhiteSpace(login) ? string.Empty : $"{login}@automind.com.br",
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

            vm.GruposSugeridos = (
                await access.SuggestAsync(
                    vm.CargoIngles ?? vm.CargoPortugues,
                    vm.Departamento,
                    cancellationToken)
                ).ToList();

            ViewBag.Ous = await ad.GetOrganizationalUnitsAsync(cancellationToken);
            return View("Novo", vm);
        }
        catch (JsonException)
        {
            TempData["TopdeskError"] = "A extensão retornou um JSON inválido do TOPdesk.";
            return RedirectToAction("Index", "Home");
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
            Email = "gabriel.silva@automind.com.br",
            TelefoneCelular = "7181696721",
            DivulgarContato = true,
            LocalTrabalho = "Salvador - Sede",
            SuperiorImediato = "Edson Neto",
            CargoPortugues = "Analista de Sistemas de Automação",
            CargoIngles = "Automation Systems Analyst",
            Departamento = "ENGENHARIA",
            GrupoTrabalho = "Engenharia",
            PerfilUsuario = "Colaborador Interno",
            GruposSugeridos = (await access.SuggestAsync("Automation Systems Analyst", "ENGENHARIA", cancellationToken)).ToList()
        };
        ViewBag.Ous = await ad.GetOrganizationalUnitsAsync(cancellationToken);
        return View("Novo", vm);
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
        var partes = Regex.Split(semAcentos.Trim(), @"\s+")
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
}
