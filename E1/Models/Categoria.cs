using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        public int id_categoria { get; set; }

        [Required]
        [StringLength(100)]
        public string nombre { get; set; } = "";

        [Required]
        [StringLength(20)]
        public string codigo { get; set; } = "";

        [StringLength(200)]
        public string? descripcion { get; set; }

        public bool estado { get; set; } = true;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}