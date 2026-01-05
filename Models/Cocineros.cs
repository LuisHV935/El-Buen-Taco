using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Buen_Taco.Models
{
    public class Cocineros
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_cocinero")]
        public int idCocinero { get; set; }
        [ForeignKey("id_empleado")]
        [Column("id_empleado")]
        public int idEmpleado { get; set; }
        [Column("especialidad")]
        public string especialidad { get; set; }
    }
}
