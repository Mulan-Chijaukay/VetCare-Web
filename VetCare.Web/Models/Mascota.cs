namespace VetCare.Web.Models
{
    public class Mascota
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public string Especie { get; set; }

        // Relación: Cada mascota tiene un dueño (Usuario)
        
        public string UsuarioId { get; set; } 
        public Usuario? Dueno { get; set; }
    }
}