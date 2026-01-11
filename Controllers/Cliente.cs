using System.Security.Claims;
using El_Buen_Taco.Data;
using El_Buen_Taco.Filtros;
using El_Buen_Taco.Models;
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
            var clienteIdClaim = User.FindFirst("ClienteId")?.Value;
            if (string.IsNullOrEmpty(clienteIdClaim) || !int.TryParse(clienteIdClaim, out var id))
            {
                return RedirectToAction("Index", "Login");
            }

            var pedidos = await _context.pedidos.Where(p => p.Idcliente == id).ToListAsync();
            pedidos.ForEach(p => p.Fecha_pedido = p.Fecha_pedido.Date);
            var cliente = await _context.clientes.FindAsync(id);
            return View(pedidos);
        }

        [BlockDirectAccess]
        [HttpGet]
        public async Task<IActionResult> HacerPedido()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> HacerPedido(Pedidos pedido)
        {
            try
            {
                var clienteIdClaim = User.FindFirst("ClienteId")?.Value;
                if (string.IsNullOrEmpty(clienteIdClaim) || !int.TryParse(clienteIdClaim, out var idCliente))
                {
                    return RedirectToAction("Index", "Login");
                }

                // Asegurar que el pedido se ligue al cliente autenticado
                pedido.Idcliente = idCliente;

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
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.pedidos.FindAsync(id);
            if (pedido != null)
            {
                _context.pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [BlockDirectAccess]
        public async Task<IActionResult> DetallesPedido(int id)
        {
            var pedido = await _context.pedidos.FindAsync(id);
            return View(pedido);
        }

        [BlockDirectAccess]
        [HttpGet]
        public async Task<IActionResult> EditarPedido(int id)
        {
            var pedido = await _context.pedidos.FindAsync(id);
            return View(pedido);
        }

        [BlockDirectAccess]
        [HttpPost]
        public async Task<IActionResult> EditarPedido(Pedidos pedido)
        {
            _context.pedidos.Update(pedido);
            await _context.SaveChangesAsync();
            ViewData["Confirmacion"] = "Pedido actualizado con exito!!!";
            return View(pedido);
        }
    }
}
