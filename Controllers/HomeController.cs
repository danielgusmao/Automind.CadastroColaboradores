using Automind.CadastroColaboradores.Services;
using Microsoft.AspNetCore.Mvc;

namespace Automind.CadastroColaboradores.Controllers;

public sealed class HomeController(IAdProvisioningWriteService adWriter) : Controller
{
    public IActionResult Index()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Usuario")))
            return RedirectToAction("Login", "Account");

        ViewBag.AdWriteMode = adWriter.IsWriteModeEnabled;
        ViewBag.AdMode = adWriter.Mode;
        return View();
    }
}
