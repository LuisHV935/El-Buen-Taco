using El_Buen_Taco.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext principal
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// *** DATA PROTECTION CRÍTICO PARA RAILWAY ***
var keysDirectory = "/app/data-protection-keys";
builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco")
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

// Configurar Antiforgery para usar Data Protection
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "ElBuenTaco.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.HeaderName = "X-CSRF-TOKEN";
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
    options.Cookie.MaxAge = TimeSpan.FromDays(14);
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
        options.Cookie.MaxAge = TimeSpan.FromDays(14);
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// *** CREAR DIRECTORIO DE CLAVES CON PERMISOS ***
if (!Directory.Exists(keysDirectory))
{
    Directory.CreateDirectory(keysDirectory);
    Console.WriteLine($"Directorio de claves creado: {keysDirectory}");
}

// Limpiar claves problemáticas si existen
try
{
    var oldKeyFiles = Directory.GetFiles(keysDirectory, "*.xml");
    if (oldKeyFiles.Length > 0)
    {
        Console.WriteLine($"Encontradas {oldKeyFiles.Length} claves viejas. Limpiando...");
        foreach (var file in oldKeyFiles)
        {
            File.Delete(file);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error limpiando claves: {ex.Message}");
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

// *** MIDDLEWARE DE EMERGENCIA PARA REGENERAR CLAVES ***
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (CryptographicException ex) when (ex.Message.Contains("The payload was invalid"))
    {
        Console.WriteLine("ERROR CRÍTICO: Regenerando claves de Data Protection...");

        // Eliminar todas las claves corruptas
        var keysPath = "/app/data-protection-keys";
        if (Directory.Exists(keysPath))
        {
            foreach (var file in Directory.GetFiles(keysPath))
            {
                File.Delete(file);
            }
        }

        // Redirigir a la página de inicio
        context.Response.Redirect("/Login/Index?error=session_expired");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();