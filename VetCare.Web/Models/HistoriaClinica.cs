using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetCare.Web.Models
{
    public class HistoriaClinica
    {
        public int Id { get; set; }

        // Relación con la Mascota
        public int MascotaId { get; set; }
        public virtual Mascota Mascota { get; set; }

        // Datos Médicos
        public DateTime FechaAtencion { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
        public string VeterinarioNombre { get; set; } // Para saber quién lo atendió
        public DateTime? ProximaCitaSugerida { get; set; }

       
        public int? CitaId { get; set; }

        [ForeignKey("CitaId")]
        public virtual Cita? Cita { get; set; }
    }
}
