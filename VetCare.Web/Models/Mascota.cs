using System.ComponentModel.DataAnnotations.Schema;

namespace VetCare.Web.Models
{
    public class Mascota
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public string Especie { get; set; }

       
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; } 
        public Usuario? Dueno { get; set; }

        
        public virtual ICollection<HistoriaClinica> HistoriasClinicas { get; set; } = new List<HistoriaClinica>();
        public int Edad { get; set; }
        public string Sexo { get; set; }
    }
}