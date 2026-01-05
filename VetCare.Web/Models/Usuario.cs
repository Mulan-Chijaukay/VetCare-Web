using System.ComponentModel.DataAnnotations;

namespace VetCare.Web.Models
{
    public class Usuario
    {
        [Key]
        public string UsuarioId { get; set; } 

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; }
    }
}