using El_Buen_Taco.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logging detallado para debug
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext principal
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CONFIGURACIÓN DATA PROTECTION SIMPLIFICADA PARA RAILWAY
// SOLO usar FileSystem, eliminar la configuración EF Core
builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco")
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys")) // Railway usa /app como raíz
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
    .ProtectKeysWithDpapi(); // Solo para desarrollo local, en Railway no hace nada

// IMPORTANTE: Configurar Forwarded Headers para Railway
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    // Necesario para Railway
    options.ForwardLimit = 2;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    // Permitir todos los proxies (Railway usa varios)
    options.AllowedHosts = new List<string> { "*" };
});

// Session - CONFIGURACIÓN CORRECTA PARA RAILWAY
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ElBuenTaco.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    // IMPORTANTE: Railway maneja HTTPS, pero internamente es HTTP
    // Usar CookieSecurePolicy.Always pero con configuración de proxy
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.MaxAge = TimeSpan.FromDays(14);
});

// Authentication - CONFIGURACIÓN CORRECTA PARA RAILWAY
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;

        // Configuración de cookies SEGURA
        options.Cookie.Name = "ElBuenTaco.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // IMPORTANTE
        options.Cookie.MaxAge = TimeSpan.FromDays(14);

        // Manejar correctamente el logout
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = context =>
            {
                // Log para debug
                Console.WriteLine($"Validando principal para: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            },
            OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Crear directorio para claves si no existe
var keysPath = "/app/keys";
if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
    Console.WriteLine($"Directorio de claves creado: {keysPath}");
}

// MIDDLEWARE ORDER ES CRÍTICO:
// 1. Forwarded Headers PRIMERO
app.UseForwardedHeaders();

// 2. Middleware para forzar HTTPS en producción (Railway lo maneja)
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        // Verificar si ya es HTTPS o si el header X-Forwarded-Proto indica HTTPS
        if (context.Request.Headers["X-Forwarded-Proto"] == "https" ||
            context.Request.IsHttps)
        {
            await next();
        }
        else
        {
            // En producción, redirigir a HTTPS
            var httpsUrl = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            context.Response.Redirect(httpsUrl, permanent: true);
        }
    });
}

// 3. Manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// 4. Static files
app.UseStaticFiles();

// 5. Routing
app.UseRouting();

// 6. Session (DEBE ir antes de Authentication)
app.UseSession();

// 7. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 8. Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

// Middleware de debug para ver cookies
app.Use(async (context, next) =>
{
    Console.WriteLine($"=== DEBUG COOKIES ===");
    Console.WriteLine($"Request Cookies: {context.Request.Cookies.Count}");
    foreach (var cookie in context.Request.Cookies)
    {
        Console.WriteLine($"  {cookie.Key}: {cookie.Value}");
    }
    Console.WriteLine($"Session ID: {context.Session.Id}");
    Console.WriteLine($"IsAuthenticated: {context.User.Identity?.IsAuthenticated}");
    Console.WriteLine($"Scheme: {context.User.Identity?.AuthenticationType}");
    Console.WriteLine($"=== END DEBUG ===");

    await next();
});

app.Run();