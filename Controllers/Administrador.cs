using El_Buen_Taco.Data;
using El_Buen_Taco.Models;
using Microsoft.AspNetCore.Http;
using El_Buen_Taco.Filtros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [BlockDirectAccess]
        public async Task<IActionResult> Index()
        {
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
            var pedidos = await _context.pedidos.ToListAsync();
            pedidos.ForEach(p => p.Fecha_pedido = p.Fecha_pedido.Date);
            return View(pedidos);
        }   

        [BlockDirectAccess]
        public async Task<IActionResult> Clientes() { 
            var clientes = await _context.clientes.ToListAsync();
            return View(clientes);  
        }

        [BlockDirectAccess]
        public IActionResult Delete(int id)
        {
            var cliente = _context.clientes.Find(id);   
            _context.clientes.Remove(cliente);   
            _context.SaveChanges();
            return View("Clientes");
        }

        [BlockDirectAccess]
        public IActionResult DeletePedido(int id, string action)
        {
            _context.pedidos.Remove(_context.pedidos.Find(id));
            _context.SaveChanges();
            return RedirectToAction(action);
        }

        [BlockDirectAccess]
        public async Task<IActionResult> DetallesPedido(int id)
        {
            var pedido = await _context.pedidos.FindAsync(id);
            int idCliente = (int)pedido.Idcliente;
            var clie = _context.clientes.Find(pedido.Idcliente);
            ViewData["ClienteName"] = clie.Name;
            ViewData["ClientePhone"] = clie.tel.ToString();
            ViewData["ClienteId"] = idCliente;
            return View(pedido);
        }
    }
}
