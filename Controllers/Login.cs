using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using El_Buen_Taco.Models;
using El_Buen_Taco.Data;
using Microsoft.EntityFrameworkCore;

namespace El_Buen_Taco.Controllers
{
    public class Login : Controller
    {
        private readonly PostgresConexion _context;

        public Login(PostgresConexion context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Clear(); 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Usuario user)
        {
            try
            {
                user.password = EncriptarContraseña.ComputeSHA256(user.password);
                var Usuario1 = _context.usuarios.FirstOrDefault(u => u.password == user.password && u.email == user.email);

                if (Usuario1 == null)
                {
                    ViewData["Mensaje"] = "❌Credenciales Invalidas";
                    return View();
                }

                // *** CAMBIO 2: Añadir claim de tiempo para evitar problemas ***
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, Usuario1.id.ToString()),
                    new Claim(ClaimTypes.Email, Usuario1.email ?? string.Empty),
                    new Claim(ClaimTypes.Name, Usuario1.email ?? string.Empty),
                    new Claim(ClaimTypes.Role, Usuario1.rol ?? string.Empty),
                    new Claim("LoginTime", DateTime.UtcNow.Ticks.ToString()) // Añadir timestamp
                };

                // Si es cliente, obtener el IdCliente y añadirlo como claim
                if (Usuario1.rol == "CLIENTE")
                {
                    var IdCliente = await _context.clientes
                        .Where(c => c.id_user == Usuario1.id)
                        .Select(c => c.Id_cliente)
                        .FirstOrDefaultAsync();

                    claims.Add(new Claim("ClienteId", IdCliente.ToString()));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Configurar propiedades de la cookie
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(6),
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                // Iniciar sesión con cookie de autenticación
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);

                // Guardar también en sesión por si acaso
                HttpContext.Session.SetString("UserId", Usuario1.id.ToString());
                HttpContext.Session.SetString("UserEmail", Usuario1.email ?? "");
                HttpContext.Session.SetString("UserRole", Usuario1.rol ?? "");

                // Redirección según rol
                switch (Usuario1.rol)
                {
                    case "ADMIN":
                        return RedirectToAction("Index", "Administrador");
                    case "CLIENTE":
                        return RedirectToAction("Index", "Cliente");
                    default:
                        return View();
                }
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = "Error: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(Registro user)
        {
            try
            {
                bool usuarioRegistrado = await _context.usuarios.AnyAsync(u => u.email == user.usuario.email);
                if (usuarioRegistrado)
                {
                    ViewData["Mensaje"] = "Email ligado a un usuario ya registrado, intente con otro";
                    return View();
                }
                else
                {
                    user.usuario.rol = "CLIENTE";
                    user.usuario.password = EncriptarContraseña.ComputeSHA256(user.usuario.password);
                    _context.usuarios.Add(user.usuario);
                    await _context.SaveChangesAsync();

                    user.cliente.id_user = user.usuario.id;
                    _context.clientes.Add(user.cliente);
                    await _context.SaveChangesAsync();

                    ViewData["Confirmacion"] = "Felicidades!!! Ya formas parte de esta gran familia 😊 \nRegresa al inicio para poder acceder a tu cuenta";
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = "Error: " + ex.Message;
                return View();
            }
        }

        // *** AÑADE ESTE MÉTODO PARA LOGOUT ***
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}