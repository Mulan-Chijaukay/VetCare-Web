using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetCare.Web.Models
{
    public class Cita
    {
        [Key]
        public int Id { get; set; }

        public string UsuarioId { get; set; }

        [Required(ErrorMessage = "Seleccione una mascota")]
        public int MascotaId { get; set; }

     
        [ForeignKey("MascotaId")]
        public virtual Mascota? Mascota { get; set; }

        [Required(ErrorMessage = "Seleccione un servicio")]
        public string Servicio { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        public string Horario { get; set; }

        public string? Observaciones { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public bool EsEmergencia { get; set; } = false;

        public int? VeterinarioId { get; set; } // FK hacia la tabla Veterinarios

        [ForeignKey("VeterinarioId")]
        public virtual Veterinario? Veterinario { get; set; } 
        public string? NombreVeterinario { get; set; }
    }
}