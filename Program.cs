using El_Buen_Taco.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext principal
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// *** DATA PROTECTION SIMPLIFICADO - SIN UseCryptographicAlgorithms ***
var keysDirectory = "/tmp/data-protection-keys"; // /tmp es persistente en Railway
builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco")
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Configurar Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "ElBuenTaco.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configurar Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "ElBuenTaco.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;

        options.Cookie.Name = "ElBuenTaco.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Crear directorio para claves
if (!Directory.Exists(keysDirectory))
{
    Directory.CreateDirectory(keysDirectory);
    Console.WriteLine($"Directorio de claves creado: {keysDirectory}");
}

// Middleware order
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();