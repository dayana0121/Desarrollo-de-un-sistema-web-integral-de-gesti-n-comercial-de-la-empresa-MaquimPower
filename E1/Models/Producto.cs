using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int id_producto { get; set; }

        public int id_categoria { get; set; }
        public int id_marca { get; set; }
        public int id_proveedor { get; set; }

        [Required]
        [StringLength(30)]
        public string sku { get; set; } = "";

        [Required]
        [StringLength(150)]
        public string nombre { get; set; } = "";

        [StringLength(300)]
        public string? descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal precio_costo { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal precio_venta { get; set; } = 0;

        public int stock_actual { get; set; } = 0;
        public int stock_minimo { get; set; } = 0;
        public bool estado { get; set; } = true;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_categoria")]
        public Categoria? Categoria { get; set; }

        [ForeignKey("id_marca")]
        public Marca? Marca { get; set; }

        [ForeignKey("id_proveedor")]
        public Proveedor? Proveedor { get; set; }
    }
}