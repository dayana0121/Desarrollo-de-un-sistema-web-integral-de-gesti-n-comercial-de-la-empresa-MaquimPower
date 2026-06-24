using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Proveedores")]
    public class Proveedor
    {
        [Key]
        public int id_proveedor { get; set; }

        [Required]
        [StringLength(11)]
        public string ruc { get; set; } = "";

        [Required]
        [StringLength(150)]
        public string razon_social { get; set; } = "";

        [StringLength(100)]
        public string? nombre_contacto { get; set; }

        [StringLength(15)]
        public string? telefono { get; set; }

        [StringLength(100)]
        public string? email { get; set; }

        [StringLength(200)]
        public string? direccion { get; set; }

        public bool estado { get; set; } = true;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}