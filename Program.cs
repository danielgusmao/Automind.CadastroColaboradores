using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Automind.CadastroColaboradores.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("MicrosoftGraph", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

if (!OperatingSystem.IsWindows())
    throw new PlatformNotSupportedException("As integracoes com Active Directory requerem o servidor IIS Windows.");

builder.Services.AddSingleton<IAdAuthenticationService, WindowsAdAuthenticationService>();
builder.Services.AddSingleton<AdConnectionFactory>();
builder.Services.AddSingleton<IAdReadOnlyService, WindowsAdReadOnlyService>();
builder.Services.AddSingleton<IProvisioningAuditService, FileProvisioningAuditService>();
builder.Services.AddSingleton<IAdProvisioningWriteService, WindowsAdProvisioningWriteService>();
builder.Services.AddSingleton<IAccessSuggestionService, WindowsAccessSuggestionService>();
builder.Services.AddSingleton<IJobTitleTranslationService, ConfigurationJobTitleTranslationService>();
builder.Services.AddSingleton<ITopdeskRequestParser, TopdeskRequestParser>();
builder.Services.AddSingleton<IMicrosoft365LicenseService, Microsoft365LicenseService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "Automind.Cadastro.LoginAD";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = false;
});
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("ad-login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
