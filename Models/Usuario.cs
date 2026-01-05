
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Buen_Taco.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_usuario")]
        public int id {  get; set; }
        [Column("email")]
        [Required(ErrorMessage = "El email es obligatorio")]

        public string email { get; set; }
        [Column("contra")]
        [Required(ErrorMessage = "Coloque una contraseña")]
        [StringLength(50,MinimumLength = 6, ErrorMessage = "Coloque una contraseña con minimo 6 caracteres")]

        public string password { get; set; }
        [Column("tipo")]
        [Required(ErrorMessage = "Seleccione un tipo")]
        public string rol { get; set; }
    }
}
