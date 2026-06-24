using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Permisos")]
    public class Permiso
    {
        [Key]
        public int id_permiso { get; set; }

        [Required]
        [StringLength(100)]
        public string nombre { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string codigo { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string modulo { get; set; } = "";

        [Required]
        [StringLength(10)]
        public string accion { get; set; } = ""; // VIEW, CREATE, EDIT, DELETE

        public bool estado { get; set; } = true;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}