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
        public async Task<IActionResult> HistorialPacientes()
        {
            var userId = _userManager.GetUserId(User);

            // Muestra las citas que este médico ya completó
            var historial = await _context.Citas
                .Include(c => c.Mascota)
                .Where(c => c.Veterinario.UsuarioId == userId && c.Estado == "Completada")
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return View(historial);
        }

        // POST: Veterinario/FinalizarConsulta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarConsulta(int citaId, string diagnostico)
        {
            var cita = await _context.Citas.FindAsync(citaId);
            if (cita != null)
            {
                cita.Estado = "Completada";
                // Aquí podrías agregar un campo 'Diagnostico' a tu tabla Citas si lo deseas
                _context.Update(cita);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Consulta finalizada con éxito.";
            }
            return RedirectToAction(nameof(Inicio));
        }
    }
}