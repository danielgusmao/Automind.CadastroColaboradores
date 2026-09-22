using Automind.CadastroColaboradores.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Fase 1: somente leitura / mocks. Nenhuma escrita no AD, TOPdesk, Entra ou Teams.
builder.Services.AddSingleton<IAdReadOnlyService, DevelopmentAdReadOnlyService>();
builder.Services.AddSingleton<ITopdeskRequestParser, TopdeskRequestParser>();
builder.Services.AddSingleton<IAccessSuggestionService, DevelopmentAccessSuggestionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
