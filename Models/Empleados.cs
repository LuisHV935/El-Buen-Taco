using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
namespace El_Buen_Taco.Models
{
    [Table("empleados")]
    public class Empleados
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_empleado")]
        public int IdEmpleado { get; set; }
        [ForeignKey("id_usuario")]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }
        [Column("nombre")]
        public string nom {  get; set; }
        [Column("salario")]
        public decimal salario { get; set; }
        [Column("tipo")]
        public string tipo { get; set; }    
    }
}
