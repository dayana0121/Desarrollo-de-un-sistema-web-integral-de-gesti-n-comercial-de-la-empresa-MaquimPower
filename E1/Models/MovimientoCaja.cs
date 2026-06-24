using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("MovimientosCaja")]
    public class MovimientoCaja
    {
        [Key]
        public int id_movimiento_caja { get; set; }

        public int id_caja { get; set; }
        public int id_usuario { get; set; }

        [Required]
        [StringLength(10)]
        public string tipo { get; set; } = ""; // INGRESO, EGRESO

        [Required]
        [StringLength(200)]
        public string concepto { get; set; } = "";

        [StringLength(20)]
        public string? tipo_referencia { get; set; }

        public int? id_referencia { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal monto { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}
