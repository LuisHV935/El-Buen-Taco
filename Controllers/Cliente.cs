using El_Buen_Taco.Data;
using El_Buen_Taco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace El_Buen_Taco.Controllers
{
    public class Cliente : Controller
    {
        private readonly PostgresConexion _context;

        public Cliente(PostgresConexion context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            int id = Convert.ToInt32(HttpContext.Session.GetInt32("ClienteId"));
            var pedidos = await _context.pedidos.Where(p => p.Idcliente == id).ToListAsync();
            return View(pedidos);  
        }
        [HttpGet]
        public async Task<IActionResult> HacerPedido(){
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> HacerPedido(Pedidos pedido)
        {
            /*try
            {*/
                await _context.pedidos.AddAsync(pedido);
                await _context.SaveChangesAsync();
                ViewData["Confirmacion"] = "Pedido registrado con exito!!!\nPronto lo tendremos listo";

            /*}
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
            }*/
            return View();
        }
    }
}
