using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Buen_Taco.Models
{
    public class Cajero
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_cajero")]
        public int idCajero { get; set; }
        [ForeignKey("id_empleado")]
        public int idEmpleado { get; set; }
        [Column("horario")]
        public string horario { get; set; }
    }
}
