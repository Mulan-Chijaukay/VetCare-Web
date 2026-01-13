using System.ComponentModel.DataAnnotations;
namespace VetCare.Web.Models;
public class Veterinario
{
    [Key]
    public int Id { get; set; } 

    [Required]
    public string Nombre { get; set; }

    public string Especialidad { get; set; }

    public string? UsuarioId { get; set; }

    public bool EstaActivo { get; set; } = true;
}