using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace El_Buen_Taco.Models
{
    [Table("moral")]
    public class Moral
    {
        [ForeignKey("id_cliente")]
        [Column("id_cliente")]
        public int Idcliente { get; set; }
        [Column("razon_social")]
        public string razonSocial { get; set; }
    }
}
