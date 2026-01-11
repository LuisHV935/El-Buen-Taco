using System.IO;
using System.Xml.Linq;
using El_Buen_Taco.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext principal
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DbContext para almacenar las claves de DataProtection
builder.Services.AddDbContext<DataProtectionKeysContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar la implementación EF concreta como scoped
builder.Services.AddScoped<El_Buen_Taco.Data.EfCoreXmlRepository>();

// Registrar el wrapper singleton
builder.Services.AddSingleton<Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository, El_Buen_Taco.Data.ScopedXmlRepositoryWrapper>();

// Configurar KeyManagementOptions
builder.Services.AddSingleton<Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>>(sp =>
    new Microsoft.Extensions.Options.ConfigureOptions<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>(opts =>
    {
        opts.XmlRepository = sp.GetRequiredService<Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository>();
    }));

// Configurar DataProtection
builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco")
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys")); // Persistir claves en Railway

// Configurar Forwarded Headers para Railway (IMPORTANTE)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Session - CONFIGURACIÓN PARA RAILWAY
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ElBuenTaco";
    // Railway maneja HTTPS externamente, internamente es HTTP
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Authentication - CONFIGURACIÓN PARA RAILWAY
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "ElBuenTacoAuth";
        options.Cookie.SameSite = SameSiteMode.Lax;
        // Railway maneja HTTPS externamente
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Crear directorio para claves si no existe
var keysPath = "/app/keys";
if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
}

// Asegurar que la tabla de claves existe
using (var scope = app.Services.CreateScope())
{
    try
    {
        var keyDb = scope.ServiceProvider.GetRequiredService<DataProtectionKeysContext>();
        keyDb.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning($"No se pudo crear tabla de claves: {ex.Message}");
    }
}

// IMPORTANTE: UseForwardedHeaders debe ir ANTES de otros middlewares
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Session debe estar antes de Authentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();