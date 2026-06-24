using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E1.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int id_cliente { get; set; }

        [Required]
        [StringLength(10)]
        public string tipo_documento { get; set; } = ""; // DNI, RUC

        [Required]
        [StringLength(11)]
        public string numero_documento { get; set; } = "";

        [Required]
        [StringLength(150)]
        public string nombre_completo { get; set; } = "";

        [StringLength(15)]
        public string? telefono { get; set; }

        [StringLength(200)]
        public string? direccion { get; set; }

        [StringLength(100)]
        public string? email { get; set; }

        public bool estado { get; set; } = true;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
        public DateTime fecha_modificacion { get; set; } = DateTime.Now;
    }
}