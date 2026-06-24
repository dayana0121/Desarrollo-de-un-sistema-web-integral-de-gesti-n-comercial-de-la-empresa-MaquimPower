using System.ComponentModel.DataAnnotations;

namespace E1.Models
{
    public class Login
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        public string username { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string password { get; set; } = "";
    }
}
