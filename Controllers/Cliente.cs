using El_Buen_Taco.Data;
using El_Buen_Taco.Filtros;
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
        [BlockDirectAccess]
        public async Task<IActionResult> Index()
        {
            int id = Convert.ToInt32(HttpContext.Session.GetInt32("ClienteId"));
            var pedidos = await _context.pedidos.Where(p => p.Idcliente == id).ToListAsync();
            pedidos.ForEach(p => p.Fecha_pedido = p.Fecha_pedido.Date);
            var cliente = await _context.clientes.FindAsync(id);    
            return View(pedidos);  
        }
        [BlockDirectAccess]
        [HttpGet]
        public async Task<IActionResult> HacerPedido(){
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> HacerPedido(Pedidos pedido)
        {
            try
            {
                await _context.pedidos.AddAsync(pedido);
                await _context.SaveChangesAsync();
                ViewData["Confirmacion"] = "Pedido registrado con exito!!!\nPronto lo tendremos listo";

            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
            }
            return View();
        }

        [BlockDirectAccess]
        public IActionResult DeletePedido(int id)
        {
            _context.pedidos.Remove(_context.pedidos.Find(id));
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [BlockDirectAccess]
        public IActionResult DetallesPedido(int id)
        {
            var pedido = _context.pedidos.Find(id);
            return View(pedido);
        }

        [BlockDirectAccess]
        [HttpGet]
        public IActionResult EditarPedido(int id)
        {
            return View(_context.pedidos.Find(id));
        }

        [BlockDirectAccess]
        [HttpPost]
        public IActionResult EditarPedido(Pedidos pedido)
        {
            _context.pedidos.Update(pedido);
            _context.SaveChanges();
            ViewData["Confirmacion"] = "Pedido actualizado con exito!!!";
            return View(pedido);
        }
        
    }
}
