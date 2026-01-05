using System.ComponentModel.DataAnnotations.Schema;

namespace El_Buen_Taco.Models
{
    [Table("fisica")]
    public class Fisica
    {
        [ForeignKey("id_cliente")]
        [Column("id_cliente")]
        public int Idcliente { get; set; }
    }
}
