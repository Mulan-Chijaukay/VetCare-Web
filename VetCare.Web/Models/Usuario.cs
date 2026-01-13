using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VetCare.Web.Models
{
    public class Usuario : IdentityUser
    {

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string Rol { get; set; }
    }
}