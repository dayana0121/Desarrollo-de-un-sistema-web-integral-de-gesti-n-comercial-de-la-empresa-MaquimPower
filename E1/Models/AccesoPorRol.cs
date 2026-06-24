using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("AccesosPorRol")]
    public class AccesoPorRol
    {
        [Key]
        public int id_acceso { get; set; }

        public int id_rol { get; set; }

        [Required]
        [StringLength(50)]
        public string modulo { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string submodulo { get; set; } = "";

        public bool tiene_acceso { get; set; } = false;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_rol")]
        public Rol? Rol { get; set; }
    }
}