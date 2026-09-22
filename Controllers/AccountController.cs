using Automind.CadastroColaboradores.Models;
using Automind.CadastroColaboradores.Services;
using Microsoft.AspNetCore.Mvc;

namespace Automind.CadastroColaboradores.Controllers;

public sealed class AccountController(IAdReadOnlyService ad) : Controller
{
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        if (!await ad.ValidateCredentialsAsync(model.Usuario, model.Senha, cancellationToken) ||
            !await ad.IsAuthorizedAsync(model.Usuario, cancellationToken))
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos, ou usuário sem acesso ao sistema.");
            return View(model);
        }

        HttpContext.Session.SetString("Usuario", model.Usuario);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
