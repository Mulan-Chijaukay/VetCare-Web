using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetCare.Web.Models
{
    public class Mascota
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Raza { get; set; }
        public string Especie { get; set; }

        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        // Relación con Historias Clínicas
        public virtual ICollection<HistoriaClinica> HistoriasClinicas { get; set; } = new List<HistoriaClinica>();

        // --- CAMBIO CLAVE AQUÍ ---

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; } = DateTime.Today.AddYears(-1); // Valor por defecto: 1 año atrás

        [NotMapped] // Esto le dice a Entity Framework que NO cree una columna "Edad" en SQL
        public int Edad
        {
            get
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - FechaNacimiento.Year;
                // Si aún no ha pasado su cumple este año, restamos uno
                if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }

        public string Sexo { get; set; } // Ejemplo: "Macho", "Hembra"
    }
}