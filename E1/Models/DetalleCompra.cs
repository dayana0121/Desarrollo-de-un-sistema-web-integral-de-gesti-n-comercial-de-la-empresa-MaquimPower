using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("DetalleCompras")]
    public class DetalleCompra
    {
        [Key]
        public int id_detalle_compra { get; set; }

        public int id_compra { get; set; }
        public int id_producto { get; set; }
        public int cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal precio_unitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal subtotal { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_producto")]
        public Producto? Producto { get; set; }
    }
}