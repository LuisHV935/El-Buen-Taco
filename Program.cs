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

var builder = WebApplication.CreateBuilder(args);

// Logging extra para DataProtection (temporal, útil para diagnóstico)
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext principal (ya existía)
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DbContext para almacenar las claves de DataProtection en la misma BD
builder.Services.AddDbContext<DataProtectionKeysContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar la implementación EF concreta como scoped
builder.Services.AddScoped<El_Buen_Taco.Data.EfCoreXmlRepository>();

// Registrar el wrapper singleton que crea scope por operación
builder.Services.AddSingleton<Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository, El_Buen_Taco.Data.ScopedXmlRepositoryWrapper>();

// Configurar KeyManagementOptions para usar el IXmlRepository inyectado (ahora seguro como singleton)
builder.Services.AddSingleton<Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>>(sp =>
    new Microsoft.Extensions.Options.ConfigureOptions<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>(opts =>
    {
        opts.XmlRepository = sp.GetRequiredService<Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository>();
    }));

// Configurar DataProtection usando el repository (ApplicationName fijo)
builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco");

// Session - CONFIGURACIÓN SEGURA PARA PRODUCCIÓN
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ElBuenTaco";

    // Cookies seguras: SameAsRequest en desarrollo, Always en producción
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;

    // Protección CSRF
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Authentication (Cookies) - CONFIGURACIÓN SEGURA PARA PRODUCCIÓN
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

        // Protección CSRF mejorada (Strict en lugar de Lax)
        options.Cookie.SameSite = SameSiteMode.Strict;

        // Cookies seguras: SameAsRequest en desarrollo, Always en producción
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Asegurar que la tabla de claves existe (si no quieres migraciones ahora)
using (var scope = app.Services.CreateScope())
{
    var keyDb = scope.ServiceProvider.GetRequiredService<DataProtectionKeysContext>();
    keyDb.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session debe estar disponible antes de MapControllers/MapControllerRoute.
app.UseSession();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();