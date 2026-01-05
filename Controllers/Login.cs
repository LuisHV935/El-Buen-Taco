
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
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Usuario user)
        {
            var Usuario1 = await _context.usuarios.FirstOrDefaultAsync(u => u.password == user.password && u.email == user.email);
            UsuariodeSesion usuariodeSesion = new UsuariodeSesion();
            if (Usuario1 != null){
                usuariodeSesion.Id = Usuario1.id;
                usuariodeSesion.Tipo = Usuario1.rol;
                usuariodeSesion.email = Usuario1.email;
                HttpContext.Session.SetInt32("UserId", usuariodeSesion.Id);
                HttpContext.Session.SetString("UserEmail", usuariodeSesion.email);
                HttpContext.Session.SetString("UserRol", usuariodeSesion.Tipo);
            }else{
                ViewData["Mensaje"] = "❌Credenciales Invalidas";
                return View();
            }

            switch (usuariodeSesion.Tipo)
            {
                case "ADMIN":
                    return RedirectToAction("Index", "Administrador");
                case "CLIENTE":
                    int IdCliente = _context.clientes.Where(c => c.id_user == usuariodeSesion.Id).Select(c => c.Id_cliente).FirstOrDefault();
                    HttpContext.Session.SetInt32("ClienteId", IdCliente);
                    return RedirectToAction("Index", "Cliente");
            }
            return View();
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
                bool usuarioRegistrado = _context.usuarios.Any(u => u.email == user.usuario.email);
                if (usuarioRegistrado)
                {
                    ViewData["Mensaje"] = "Email ligado a un usuario ya registrado, intente con otro";
                    return View();
                }
                else
                {
                    user.usuario.rol = "CLIENTE";
                    _context.usuarios.Add(user.usuario);
                    _context.SaveChanges();
                    user.cliente.id_user = user.usuario.id;
                    _context.clientes.Add(user.cliente);
                    _context.SaveChanges();
                    ViewData["Confirmacion"] = "Felicidades!!! Ya formas parte de esta gran familia 😊 \nRegresa al inicio para poder acceder a tu cuenta";
                }
                return View();

            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
                return View();
            }
        }
    }
}
