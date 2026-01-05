using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetCare.Web.Models; //para reconocer la clase Usuario

namespace VetCare.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Cita> Citas { get; set; } 
    }
}