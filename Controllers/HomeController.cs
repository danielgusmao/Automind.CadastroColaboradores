using Microsoft.AspNetCore.Mvc;

namespace Automind.CadastroColaboradores.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Usuario")))
            return RedirectToAction("Login", "Account");
        return View();
    }
}
