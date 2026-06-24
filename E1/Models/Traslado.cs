using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Traslados")]
    public class Traslado
    {
        [Key]
        public int id_traslado { get; set; }

        public int id_usuario { get; set; }
        public int id_almacen_origen { get; set; }
        public int id_almacen_destino { get; set; }

        [Required]
        [StringLength(20)]
        public string numero_traslado { get; set; } = "";

        public DateTime fecha_traslado { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string estado { get; set; } = "Pendiente";

        [StringLength(300)]
        public string? observacion { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_almacen_origen")]
        public Almacen? AlmacenOrigen { get; set; }

        [ForeignKey("id_almacen_destino")]
        public Almacen? AlmacenDestino { get; set; }
    }
}