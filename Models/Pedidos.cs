
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace El_Buen_Taco.Models
{
    public class Pedidos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_pedido")]
        public int Idpedido { get; set; }
        [ForeignKey("id_cliente")]
        [Column("id_cliente")]
        public int Idcliente { get; set; }
        [Column("alimentos")]
        public string alimentos { get; set; }
        [Column("hora_de_entrega")]
        public TimeSpan horadeentrega {  get; set; }
        [Column("metodo_de_pago")]
        public string metododePago { get; set; }
        [Column("tipo")]
        public string tipo {  get; set; }
        [Column("fecha_pedido")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Fecha_pedido { get; set; }
    }

}
