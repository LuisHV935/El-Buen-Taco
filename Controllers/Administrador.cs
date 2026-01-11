using System.Security.Claims;
using El_Buen_Taco.Data;
using El_Buen_Taco.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using El_Buen_Taco.Filtros;
using System.Numerics;

namespace El_Buen_Taco.Controllers
{
    public class Administrador : Controller
    {
        private readonly PostgresConexion _context;
        public Administrador(PostgresConexion context)
        {
            _context = context;
        }

        private bool IsAdminAuthenticated()
        {
            return User?.Identity?.IsAuthenticated == true && User.IsInRole("ADMIN");
        }

        [BlockDirectAccess]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            DateTime hoy = DateTime.Today;

            var pedidos = _context.pedidos
                .AsEnumerable()
                .Where(p => p.Fecha_pedido.Year == hoy.Year &&
                            p.Fecha_pedido.Month == hoy.Month &&
                            p.Fecha_pedido.Day == hoy.Day)
                .ToList();
            return View(pedidos);
        }

        [BlockDirectAccess]
        public async Task<IActionResult> Pedidos()
        {
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            var pedidos = await _context.pedidos.ToListAsync();
            pedidos.ForEach(p => p.Fecha_pedido = p.Fecha_pedido.Date);
            return View(pedidos);
        }

        [BlockDirectAccess]
        public async Task<IActionResult> Clientes()
        {
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            var clientes = await _context.clientes.ToListAsync();
            return View(clientes);
        }

        [BlockDirectAccess]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            var cliente = await _context.clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Clientes");
        }

        [BlockDirectAccess]
        public async Task<IActionResult> DeletePedido(int id, string action)
        {
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            var pedidoAEliminar = await _context.pedidos.FindAsync(id);
            if (pedidoAEliminar != null)
            {
                _context.pedidos.Remove(pedidoAEliminar);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(action);
        }

        [BlockDirectAccess]
        public async Task<IActionResult> DetallesPedido(int id)
        {
            if (!IsAdminAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            var pedido = await _context.pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            int idCliente = (int)pedido.Idcliente;
            var clie = await _context.clientes.FindAsync(pedido.Idcliente);
            ViewData["ClienteName"] = clie?.Name;
            ViewData["ClientePhone"] = clie?.tel.ToString();
            ViewData["ClienteId"] = idCliente;
            return View(pedido);
        }
    }
}