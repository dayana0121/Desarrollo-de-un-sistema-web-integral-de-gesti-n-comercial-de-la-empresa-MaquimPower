using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Empresa")]
    public class Empresa
    {
        [Key]
        public int id_empresa { get; set; }

        [Required]
        [StringLength(150)]
        public string nombre { get; set; } = "";

        [Required]
        [StringLength(11)]
        public string ruc { get; set; } = "";

        [StringLength(200)]
        public string? direccion { get; set; }

        [StringLength(15)]
        public string? telefono { get; set; }

        [StringLength(100)]
        public string? email { get; set; }

        [StringLength(300)]
        public string? logo_url { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}
