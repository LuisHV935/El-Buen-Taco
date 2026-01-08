using El_Buen_Taco.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Data Protection para Render (ESENCIAL)
var keysPath = Path.Combine(Path.GetTempPath(), "aspnet-keys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("El Buen Taco");

// 2. Servicios
builder.Services.AddControllersWithViews();

// 3. Base de datos
builder.Services.AddDbContext<PostgresConexion>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Sesión CONFIGURADA CORRECTAMENTE
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ElBuenTaco";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS en Render
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 5. HttpContext
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 6. Pipeline - ORDEN CORRECTO
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ORDEN CRÍTICO:
app.UseSession(); // ← PRIMERO
app.UseRouting();
app.UseAuthentication(); // ← NO OLVIDES ESTA
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

// 7. Puerto para Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");