using El_Buen_Taco.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logging detallado para debug
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext principal
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DATA PROTECTION SIMPLIFICADA - SIN DPAPI (Linux compatible)
builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco")
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
// ¡NO usar ProtectKeysWithDpapi() en Linux/Railway!

// Configurar Forwarded Headers para Railway
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = 2;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Session - Configuración para Railway (Linux)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ElBuenTaco.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Railway maneja HTTPS externamente
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.MaxAge = TimeSpan.FromDays(14);
});

// Authentication - SOLO configuración básica
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";  // Solo esta ruta existe
        // AccessDeniedPath es opcional, si no tienes página de acceso denegado, omítelo
        // options.AccessDeniedPath = "/Login/Index"; // Solo si tienes esa página
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

// Crear directorio para claves si no existe
var keysPath = "/app/keys";
if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
    Console.WriteLine($"Directorio de claves creado: {keysPath}");

    // Crear un archivo .gitkeep para que Railway no borre el directorio
    File.WriteAllText(Path.Combine(keysPath, ".gitkeep"), "");
}

// IMPORTANTE: Eliminar claves viejas si existen problemas
try
{
    var oldKeys = Directory.GetFiles(keysPath, "*.xml");
    foreach (var oldKey in oldKeys)
    {
        Console.WriteLine($"Eliminando clave vieja: {oldKey}");
        File.Delete(oldKey);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error limpiando claves viejas: {ex.Message}");
}

// MIDDLEWARE ORDER
app.UseForwardedHeaders();

// En producción, usar HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// Session antes de Authentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Endpoint de debug (temporal) - opcional
app.MapGet("/debug/keys", () =>
{
    var keysPath = "/app/keys";
    if (!Directory.Exists(keysPath))
        return "No existe directorio de claves";

    var files = Directory.GetFiles(keysPath);
    return $"Archivos en {keysPath}: {string.Join(", ", files)}";
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();