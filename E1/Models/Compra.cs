using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Compras")]
    public class Compra
    {
        [Key]
        public int id_compra { get; set; }

        public int id_proveedor { get; set; }
        public int id_usuario { get; set; }
        public int id_almacen { get; set; }

        [Required]
        [StringLength(20)]
        public string numero_compra { get; set; } = "";

        public DateTime fecha_compra { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal subtotal { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal igv { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal total { get; set; } = 0;

        [StringLength(20)]
        public string estado { get; set; } = "Pendiente";

        [StringLength(300)]
        public string? observacion { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_proveedor")]
        public Proveedor? Proveedor { get; set; }

        [ForeignKey("id_almacen")]
        public Almacen? Almacen { get; set; }
    }
}