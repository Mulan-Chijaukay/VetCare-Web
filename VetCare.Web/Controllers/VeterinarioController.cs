using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Web.Data;
using VetCare.Web.Models;

namespace VetCare.Web.Controllers
{
    [Authorize(Roles = "Veterinario")]
    public class VeterinarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public VeterinarioController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Veterinario/Inicio
        public async Task<IActionResult> Inicio(DateTime? fecha, string vista = "Dia")
        {
            var userId = _userManager.GetUserId(User);
            DateTime fechaBase = fecha ?? DateTime.Today;

            // Configuración de la vista
            ViewBag.FechaActual = fechaBase;
            ViewBag.Vista = vista;

            IQueryable<Cita> query = _context.Citas
                .Include(c => c.Mascota)
                    .ThenInclude(m => m.Usuario)
                .Where(c => c.Veterinario.UsuarioId == userId && c.Estado != "Cancelada");

            if (vista == "Semana")
            {
                // Calcular el lunes de esa semana
                int diff = (7 + (fechaBase.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime lunes = fechaBase.AddDays(-1 * diff).Date;
                DateTime domingo = lunes.AddDays(7).Date;

                query = query.Where(c => c.Fecha >= lunes && c.Fecha < domingo);
                ViewBag.InicioSemana = lunes;
            }
            else
            {
                query = query.Where(c => c.Fecha.Date == fechaBase.Date);
            }

            var citas = await query.OrderBy(c => c.Fecha).ThenBy(c => c.Horario).ToListAsync();
            return View(citas);
        }



        // GET: Veterinario/HistorialPacientes
        public async Task<IActionResult> HistorialPacientes(string searchString)
        {
            var userId = _userManager.GetUserId(User);

            // Consultamos la tabla de HistoriasClinicas que acabamos de crear
            var query = _context.HistoriasClinicas
                .Include(h => h.Mascota)
                    .ThenInclude(m => m.Usuario)
                .AsQueryable();

            // Filtro de búsqueda por nombre de mascota o dueño
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.Mascota.Nombre.Contains(searchString) ||
                                         h.Mascota.Usuario.Nombre.Contains(searchString));
            }

            var historial = await query.OrderByDescending(h => h.FechaAtencion).ToListAsync();
            return View(historial);
        }



        //  vista "Mis Consultas"
        public async Task<IActionResult> MisConsultas()
        {
            var userId = _userManager.GetUserId(User);
            var misAtenciones = await _context.HistoriasClinicas
                .Include(h => h.Mascota)
                    .ThenInclude(m => m.Usuario)
                .Include(h => h.Cita) // <--- Agrega esto para conectar la historia con la cita
                    .ThenInclude(c => c.Veterinario) // <--- Y esto para llegar al veterinario
                .Where(h => h.Cita.Veterinario.UsuarioId == userId)
                .OrderByDescending(h => h.FechaAtencion)
                .ToListAsync();

            return View(misAtenciones);
        }

        // método FinalizarConsulta 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarConsulta(int citaId, string diagnostico, string tratamiento, DateTime? proximaCitaSugerida)
        {
            var userId = _userManager.GetUserId(User);
            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.Veterinario)
                .FirstOrDefaultAsync(c => c.Id == citaId);

            if (cita == null) return NotFound();

            if (cita.Veterinario?.UsuarioId != userId)
            {
                return Forbid();
            }

            // Lógica de creación de HistoriaClinica
            var entradaHistoria = new HistoriaClinica
            {
                MascotaId = cita.MascotaId,
                FechaAtencion = DateTime.Now,
                Diagnostico = diagnostico,
                Tratamiento = tratamiento,
                VeterinarioNombre = cita.Veterinario?.NombreCompleto ?? "Veterinario",
                CitaId = cita.Id,
                ProximaCitaSugerida = proximaCitaSugerida // <--- DATO CLAVE GUARDADO
            };

            cita.Estado = "Completada";

            _context.HistoriasClinicas.Add(entradaHistoria);
            _context.Citas.Update(cita);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Inicio));
        }



        public async Task<IActionResult> DetallePaciente(int id)
        {
            var mascota = await _context.Mascotas
                .Include(m => m.Usuario) // Dueño
                .Include(m => m.HistoriasClinicas.OrderByDescending(h => h.FechaAtencion))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mascota == null) return NotFound();

            return View(mascota);
        }



    }
}