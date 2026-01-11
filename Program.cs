using El_Buen_Taco.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// *** CONFIGURACIÓN DATA PROTECTION PARA RENDER ***
// Render tiene almacenamiento persistente en /var/data
var dataProtectionPath = Path.Combine(
    Environment.GetEnvironmentVariable("RENDER_DATA_PATH") ?? "/var/data",
    "data-protection-keys");

builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".ElBuenTaco.Session";
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
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;

        options.Cookie.Name = ".ElBuenTaco.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

var app = builder.Build();

// Crear directorio de claves
if (!Directory.Exists(dataProtectionPath))
{
    Directory.CreateDirectory(dataProtectionPath);
    Console.WriteLine($"✓ Directorio de claves creado: {dataProtectionPath}");
}

// Verificar claves existentes
var existingKeys = Directory.GetFiles(dataProtectionPath, "*.xml");
Console.WriteLine($"✓ Claves existentes: {existingKeys.Length}");

// *** SI HAY CLAVES, ELIMINARLAS PARA FORZAR NUEVAS ***
if (existingKeys.Length > 0)
{
    Console.WriteLine("⚠️ Eliminando claves viejas...");
    foreach (var keyFile in existingKeys)
    {
        File.Delete(keyFile);
    }
    Console.WriteLine("✓ Claves viejas eliminadas. Se crearán nuevas.");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// *** ENDPOINT DE EMERGENCIA PARA RENDER ***
app.MapGet("/reset-cookies", async (HttpContext context) =>
{
    // Eliminar cookies problemáticas
    context.Response.Cookies.Delete(".ElBuenTaco.Auth");
    context.Response.Cookies.Delete(".ElBuenTaco.Session");

    // Redirigir al login
    context.Response.Redirect("/Login/Index");
    return Task.CompletedTask;
});

// *** ENDPOINT PARA VER ESTADO ***
app.MapGet("/render-debug", () =>
{
    var dataPath = Environment.GetEnvironmentVariable("RENDER_DATA_PATH") ?? "/var/data";
    var keysPath = Path.Combine(dataPath, "data-protection-keys");

    var keysExist = Directory.Exists(keysPath);
    var keyCount = keysExist ? Directory.GetFiles(keysPath, "*.xml").Length : 0;

    return Results.Ok(new
    {
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        dataPath = dataPath,
        keysPath = keysPath,
        keysExist = keysExist,
        keyCount = keyCount,
        timestamp = DateTime.UtcNow
    });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();