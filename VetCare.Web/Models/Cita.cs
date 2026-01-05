

using System.ComponentModel.DataAnnotations;

namespace VetCare.Web.Models
{
    public class Cita
    {
        [Key]
        public int Id { get; set; }

        // Debe ser string para que coincida con el ID de Identity
        public string UsuarioId { get; set; }

        [Required(ErrorMessage = "Seleccione una mascota")]
        public int MascotaId { get; set; }

        [Required(ErrorMessage = "Seleccione un servicio")]
        public string Servicio { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        public string Horario { get; set; }

        public string Observaciones { get; set; }

        // Campo para manejar el estado (Pendiente, Confirmada, etc.)
        public string Estado { get; set; } = "Pendiente";

        // NUEVO: Campo para identificar emergencias
        public bool EsEmergencia { get; set; } = false;
    }
}