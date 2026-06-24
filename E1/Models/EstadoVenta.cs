using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("EstadosVenta")]
    public class EstadoVenta
    {
        [Key]
        public int id_estado_venta { get; set; }

        [Required]
        [StringLength(50)]
        public string nombre { get; set; } = "";

        [Required]
        [StringLength(20)]
        public string codigo { get; set; } = "";

        public bool estado { get; set; } = true;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}