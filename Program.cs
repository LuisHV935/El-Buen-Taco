using El_Buen_Taco.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// *** 1. DATA PROTECTION EN MEMORIA (TEMPORAL) ***
// Esto evita el problema de claves corruptas persistentes
builder.Services.AddDataProtection()
    .SetApplicationName("El_Buen_Taco")
    .SetDefaultKeyLifetime(TimeSpan.FromHours(4))
    .DisableAutomaticKeyGeneration(); // Importante: no genera claves automáticamente

// *** 2. CONFIGURAR ANTIFORGERY PARA IGNORAR TOKENS CORRUPTOS ***
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "ElBuenTaco.Csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Temporal
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = true;
});

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// *** 3. SESSION CON NOMBRE ÚNICO ***
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = "ElBuenTaco.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Temporal para Render
});

// *** 4. AUTHENTICATION CON NOMBRE ÚNICO ***
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(6);
        options.SlidingExpiration = true;

        options.Cookie.Name = "ElBuenTaco.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Temporal

        // *** 5. MANEJAR ERRORES DE COOKIE CORRUPTA ***
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                // Si la cookie es corrupta, forzar logout
                try
                {
                    // Intenta acceder a claims
                    var name = context.Principal?.Identity?.Name;
                }
                catch
                {
                    // Cookie corrupta - rechazar
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

var app = builder.Build();

// *** 6. MIDDLEWARE PARA ELIMINAR COOKIES CORRUPTAS AL ENTRAR A LOGIN ***
app.Use(async (context, next) =>
{
    // Cada vez que alguien visita login, limpiar posibles cookies corruptas
    if (context.Request.Path.StartsWithSegments("/Login"))
    {
        context.Response.OnStarting(() =>
        {
            // Eliminar TODAS las cookies posibles
            var cookiesToDelete = new[]
            {
                "ElBuenTaco.Auth",
                "ElBuenTaco.Session",
                "ElBuenTaco.Csrf",
                ".AspNetCore.Antiforgery",
                ".AspNetCore.Cookies",
                ".AspNetCore.Session"
            };

            foreach (var cookieName in cookiesToDelete)
            {
                context.Response.Cookies.Delete(cookieName);
            }

            return Task.CompletedTask;
        });
    }

    await next();
});

// Middleware estándar
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// *** 7. ENDPOINT DE EMERGENCIA ***
app.MapGet("/emergency-reset", async (HttpContext context) =>
{
    // Cerrar sesión
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    // Eliminar todas las cookies
    context.Response.Cookies.Delete("ElBuenTaco.Auth");
    context.Response.Cookies.Delete("ElBuenTaco.Session");
    context.Response.Cookies.Delete("ElBuenTaco.Csrf");

    // Limpiar sesión
    context.Session.Clear();

    // Redirigir al login
    context.Response.Redirect("/Login/Index?reset=success");
    return Task.CompletedTask;
});

// *** 8. ENDPOINT PARA DEBUG ***
app.MapGet("/debug-cookies", (HttpContext context) =>
{
    var result = new
    {
        RequestCookies = context.Request.Cookies.Keys.ToList(),
        SessionId = context.Session.Id,
        IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
        UserName = context.User.Identity?.Name,
        Time = DateTime.UtcNow
    };

    return Results.Json(result);
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();