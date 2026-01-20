namespace VetCare.Web.Models
{
    public class DashboardViewModel
    {

        public int CitasHoy { get; set; }
        public int CitasPendientes { get; set; }
        public int TotalMascotas { get; set; }

        // Estados del sistema (Lo nuevo)
        public bool DbOnline { get; set; }
        public bool AppActiva { get; set; }
    }
}
