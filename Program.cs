using El_Buen_Taco.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// *** DATA PROTECTION CON DURACIÓN MÍNIMA DE 1 SEMANA ***
builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(7)); // ¡MÍNIMO 7 DÍAS!

// Services básicos
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session - SIN configuración compleja
builder.Services.AddSession();

// Authentication - CONFIGURACIÓN MÍNIMA
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;

        // Solo lo esencial
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Temporal para desarrollo
    });

var app = builder.Build();

// *** MIDDLEWARE DE EMERGENCIA: Si hay error de antiforgery, limpiar cookies ***
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException)
    {
        // Error de antiforgery - limpiar cookies y redirigir
        context.Response.Cookies.Delete(".AspNetCore.Antiforgery");
        context.Response.Cookies.Delete(".AspNetCore.Cookies");
        context.Response.Redirect("/Login/Index?error=session_expired");
    }
    catch (System.Security.Cryptography.CryptographicException ex)
        when (ex.Message.Contains("The payload was invalid"))
    {
        // Error de Data Protection - limpiar cookies
        context.Response.Cookies.Delete(".AspNetCore.Cookies");
        context.Response.Cookies.Delete(".AspNetCore.Session");
        context.Response.Redirect("/Login/Index?error=security_reset");
    }
});

// Middleware estándar
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// *** ENDPOINT SIMPLE PARA DIAGNÓSTICO ***
app.MapGet("/status", () =>
{
    return Results.Ok(new
    {
        status = "running",
        timestamp = DateTime.UtcNow,
        data_protection = "configured"
    });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();