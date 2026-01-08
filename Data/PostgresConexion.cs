using El_Buen_Taco.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
namespace El_Buen_Taco.Data
{
    public class PostgresConexion : DbContext
    {
        public PostgresConexion(DbContextOptions<PostgresConexion> options) : base(options)
        {
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Todas las propiedades DateTime se manejarán como UTC
            configurationBuilder.Properties<DateTime>()
                .HaveConversion<DateTimeToUtcConverter>();
        }

        public class DateTimeToUtcConverter : ValueConverter<DateTime, DateTime>
        {
            public DateTimeToUtcConverter()
                : base(
                    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            {
            }
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
