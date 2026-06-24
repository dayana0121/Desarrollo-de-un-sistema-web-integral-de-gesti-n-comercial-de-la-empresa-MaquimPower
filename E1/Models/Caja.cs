using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Cajas")]
    public class Caja
    {
        [Key]
        public int id_caja { get; set; }

        public int id_usuario { get; set; }

        [Required]
        [StringLength(100)]
        public string nombre_caja { get; set; } = "";

        [Column(TypeName = "decimal(10,2)")]
        public decimal monto_apertura { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? monto_cierre { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? monto_esperado { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? diferencia { get; set; }

        [StringLength(20)]
        public string estado { get; set; } = "Abierta";

        public DateTime fecha_apertura { get; set; } = DateTime.Now;
        public DateTime? fecha_cierre { get; set; }

        [StringLength(300)]
        public string? observacion { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}