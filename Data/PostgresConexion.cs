using El_Buen_Taco.Models;
using Microsoft.EntityFrameworkCore;
namespace El_Buen_Taco.Data
{
    public class PostgresConexion : DbContext
    {
        public PostgresConexion(DbContextOptions<PostgresConexion> options) : base(options)
        {
        }

        public DbSet<Usuario> usuarios { get; set; }
        public DbSet<Cliente> clientes { get; set; } 
        public DbSet<Empleados> empleados { get; set; } 
        public DbSet<Repartidores> repartidores { get; set; }   
        public DbSet<Pedidos> pedidos { get; set; }
        public DbSet<Cocineros> cocinero {  get; set; }
        public DbSet<Cajero> cajeros { get; set; }
    }
}
