using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("MovimientosStock")]
    public class MovimientoStock
    {
        [Key]
        public int id_movimiento { get; set; }

        public int id_producto { get; set; }
        public int id_almacen { get; set; }
        public int id_usuario { get; set; }

        [Required]
        [StringLength(10)]
        public string tipo_movimiento { get; set; } = ""; // ENTRADA, SALIDA

        [Required]
        [StringLength(20)]
        public string tipo_referencia { get; set; } = ""; // COMPRA, VENTA, TRASLADO, AJUSTE

        public int? id_referencia { get; set; }
        public int cantidad { get; set; }
        public int stock_anterior { get; set; }
        public int stock_resultante { get; set; }

        [StringLength(200)]
        public string? observacion { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_producto")]
        public Producto? Producto { get; set; }

        [ForeignKey("id_almacen")]
        public Almacen? Almacen { get; set; }
    }
}