using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace El_Buen_Taco.Models
{
    [Table("cliente")]
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_cliente")]
        public int Id_cliente { get; set; }
        [Column("nombre")]
        public string Name { get; set; }
        [Column("telefono")]
        public long tel { get; set; }
        [Column("tipo")]
        public string tipo { get; set; }
        [ForeignKey("id_usuario")]
        [Column("id_usuario")]
        public int id_user { get; set; }

    }
}
