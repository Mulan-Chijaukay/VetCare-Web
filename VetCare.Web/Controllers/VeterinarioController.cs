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






        // POST: Veterinario/FinalizarConsulta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarConsulta(int citaId, string diagnostico, string tratamiento)
        {
            // Obtenemos el ID del veterinario actual para seguridad
            var userId = _userManager.GetUserId(User);

            // 1. Buscamos la cita incluyendo las relaciones necesarias
            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.Veterinario)
                .FirstOrDefaultAsync(c => c.Id == citaId);

            if (cita == null) return NotFound();

            // --- VALIDACIÓN DE SEGURIDAD Y DUPLICADOS ---
            // Verificamos que la cita le pertenezca a este veterinario
            if (cita.Veterinario?.UsuarioId != userId)
            {
                TempData["Error"] = "No tienes permiso para finalizar esta consulta.";
                return RedirectToAction(nameof(Inicio));
            }

            // Verificamos si ya está completada para no duplicar historia clínica
            if (cita.Estado == "Completada")
            {
                TempData["Mensaje"] = "Esta consulta ya fue registrada anteriormente.";
                return RedirectToAction(nameof(Inicio));
            }
            // --------------------------------------------

            // 2. Creamos el registro en la Historia Clínica
            var entradaHistoria = new HistoriaClinica
            {
                MascotaId = cita.MascotaId,
                FechaAtencion = DateTime.Now,
                Diagnostico = diagnostico,
                Tratamiento = tratamiento,
                VeterinarioNombre = cita.Veterinario?.NombreCompleto ?? "Veterinario",
                CitaId = cita.Id
            };

            // 3. Actualizamos el estado de la cita
            cita.Estado = "Completada";
            cita.Diagnostico = diagnostico;
            cita.Tratamiento = tratamiento; // Asegúrate de que Cita tenga este campo también

            try
            {
                _context.HistoriasClinicas.Add(entradaHistoria);
                _context.Citas.Update(cita);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Consulta finalizada. Se ha generado un nuevo registro en la Historia Clínica.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hubo un error al guardar en la base de datos.";
            }

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