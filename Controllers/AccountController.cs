using System.Security.Claims;
using Automind.CadastroColaboradores.Models;
using Automind.CadastroColaboradores.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Automind.CadastroColaboradores.Controllers;

public sealed class AccountController(IAdAuthenticationService ad, ILogger<AccountController> logger) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("ad-login")]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return LoginFailure(model);
        try
        {
            if (await ad.AuthenticateAsync(model.Usuario, model.Senha, cancellationToken))
            {
                var usuario = model.Usuario.Trim();
                var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, usuario)],
                    CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("Usuario", usuario);
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos, ou usuário sem acesso ao grupo autorizado no AD.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            // Não registrar usuário/senha, model ou mensagens do provedor que possam conter dados sensíveis.
            logger.LogWarning("Falha no login AD. Tipo: {ErrorType}; código: {Code}", exception.GetType().Name, exception.HResult);
            ModelState.AddModelError(string.Empty, "Não foi possível validar o acesso no AD. Verifique conexão com o domínio e permissão de leitura da identidade do pool.");
        }
        return LoginFailure(model);
    }

    private IActionResult LoginFailure(LoginViewModel model)
    {
        model.Senha = string.Empty;
        ModelState.SetModelValue(nameof(model.Senha), string.Empty, string.Empty);
        return View("Login", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
