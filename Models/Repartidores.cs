using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace El_Buen_Taco.Models
{
    [Table("repartidores")]
    public class Repartidores
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_repartidor")]
        public int idRepartidor {  get; set; }
        [ForeignKey("id_empleado")]
        [Column("id_empleado")]
        public int idEmpleado { get; set; }
        [Column("vehiculo")]
        public string vehiculo { get; set; }
    }
}
