using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Cotizaciones")]
    public class Cotizacion
    {
        [Key]
        public int id_cotizacion { get; set; }

        public int id_cliente { get; set; }
        public int id_usuario { get; set; }

        [Required]
        [StringLength(20)]
        public string numero_cotizacion { get; set; } = "";

        public DateTime fecha_cotizacion { get; set; } = DateTime.Now;

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
        [ForeignKey("id_cliente")]
        public Cliente? Cliente { get; set; }
    }
}