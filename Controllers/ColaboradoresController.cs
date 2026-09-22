using System.Globalization;
using System.Text;
using Automind.CadastroColaboradores.Models;
using Automind.CadastroColaboradores.Services;
using Microsoft.AspNetCore.Mvc;

namespace Automind.CadastroColaboradores.Controllers;

public sealed class ColaboradoresController(IAdReadOnlyService ad, IAccessSuggestionService access) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Novo(CancellationToken cancellationToken)
    {
        ViewBag.Ous = await ad.GetOrganizationalUnitsAsync(cancellationToken);
        return View(new CollaboratorFormViewModel());
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

    private static string NormalizarNome(string nome)
    {
        var textInfo = new CultureInfo("pt-BR").TextInfo;
        var lower = nome.Trim().ToLower(new CultureInfo("pt-BR"));
        var result = textInfo.ToTitleCase(lower);
        foreach (var p in new[] { " Da ", " De ", " Do ", " Das ", " Dos " })
            result = result.Replace(p, p.ToLowerInvariant());
        return result;
    }
}
