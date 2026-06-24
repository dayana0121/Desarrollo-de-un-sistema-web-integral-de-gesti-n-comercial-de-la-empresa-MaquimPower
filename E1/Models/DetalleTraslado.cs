using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("DetalleTraslados")]
    public class DetalleTraslado
    {
        [Key]
        public int id_detalle_traslado { get; set; }

        public int id_traslado { get; set; }
        public int id_producto { get; set; }
        public int cantidad { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_producto")]
        public Producto? Producto { get; set; }
    }
}