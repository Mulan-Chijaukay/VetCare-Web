using System.ComponentModel.DataAnnotations;

public class Veterinario
{
    [Key]
    public int Id { get; set; } // ID numérico para relaciones rápidas en la BD

    [Required]
    public string Nombre { get; set; }

    public string Especialidad { get; set; }

    // VINCULO CON IDENTITY: Aqui se guardara el ID string del usuario
    public string? UsuarioId { get; set; }

    public bool EstaActivo { get; set; } = true;
}