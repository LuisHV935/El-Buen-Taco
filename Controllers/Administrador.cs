using El_Buen_Taco.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace El_Buen_Taco.Controllers
{
    public class Administrador : Controller
    {
        private readonly PostgresConexion _context;
        public Administrador(PostgresConexion context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.pedidos.ToListAsync();
            return View(pedidos);
        }

        public async Task<IActionResult> Clientes() { 
            var clientes = await _context.clientes.ToListAsync();
            return View(clientes);  
        }

        public IActionResult Delete(int id)
        {
            _context.clientes.Remove(_context.clientes.Find(id));   
            _context.SaveChanges();
            return View("Clientes");
        }

        public IActionResult DeletePedido(int id)
        {
            _context.pedidos.Remove(_context.pedidos.Find(id));
            _context.SaveChanges();
            return View("Index");
        }
    }
}
