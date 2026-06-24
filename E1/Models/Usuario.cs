using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int id_usuario { get; set; }

        public int id_rol { get; set; }

        [Required]
        [StringLength(100)]
        public string nombre { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string username { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string password_hash { get; set; } = "";

        public bool bloqueado { get; set; } = false;
        public bool estado { get; set; } = true;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("id_rol")]
        public Rol? Rol { get; set; }
    }
}