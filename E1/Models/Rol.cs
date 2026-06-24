using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Roles")]
    public class Rol
    {
        [Key]
        public int id_rol { get; set; }

        [Required]
        [StringLength(50)]
        public string nombre { get; set; } = "";

        public bool es_sistema { get; set; } = false;
        public bool estado { get; set; } = true;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}