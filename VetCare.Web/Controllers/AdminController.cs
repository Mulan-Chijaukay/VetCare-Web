using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetCare.Web.Data;
using VetCare.Web.Models;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var hoy = DateTime.Today;

        // Creamos el modelo y lo llenamos con los datos reales
        var model = new DashboardViewModel
        {
            CitasHoy = await _context.Citas.CountAsync(c => c.Fecha.Date == hoy),
            CitasPendientes = await _context.Citas.CountAsync(c => c.Estado == "Pendiente"),
            TotalMascotas = await _context.Mascotas.CountAsync(),

            // PRUEBA REAL DE BASE DE DATOS
            DbOnline = await _context.Database.CanConnectAsync(),
            AppActiva = true // Si el código llega aquí, la app está viva
        };

        return View(model);
    }

    public async Task<IActionResult> GestionCitas()
    {
        var citas = await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Veterinario)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();

        ViewBag.Veterinarios = new SelectList(await _context.Veterinarios.Where(v => v.EstaActivo).ToListAsync(), "Id", "Nombre");
        return View(citas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarVeterinario(int citaId, int veterinarioId)
    {
        var cita = await _context.Citas.FindAsync(citaId);
        var vet = await _context.Veterinarios.FindAsync(veterinarioId);

        if (cita != null && vet != null)
        {
            cita.VeterinarioId = veterinarioId;
            cita.NombreVeterinario = vet.Nombre;
            cita.Estado = "Confirmada";
            _context.Update(cita);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Médico asignado correctamente.";
        }
        return RedirectToAction(nameof(GestionCitas));
    }

    [HttpPost]
    public async Task<IActionResult> EliminarCita(int id)
    {
        // 1. Buscamos si la cita tiene historias clínicas asociadas
        var historias = _context.HistoriasClinicas.Where(h => h.CitaId == id);
        if (historias.Any())
        {
            _context.HistoriasClinicas.RemoveRange(historias);
        }

        // 2. Ahora sí borramos la cita
        var cita = await _context.Citas.FindAsync(id);
        if (cita != null)
        {
            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(GestionCitas));
    }

    public async Task<IActionResult> ListaVeterinarios()
    {
        var vete = await _context.Veterinarios.ToListAsync();
        return View(vete);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarVeterinario(string nombre, string especialidad)
    {
        if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(especialidad))
        {
            string emailAcceso = nombre.Replace(" ", "").ToLower() + "@vetcare.com";
            var nuevoUsuario = new Usuario
            {
                UserName = emailAcceso,
                Email = emailAcceso,
                Nombre = nombre,
                Rol = "Veterinario",
                EmailConfirmed = true
            };

            var resultadoIdentity = await _userManager.CreateAsync(nuevoUsuario, "VetCare2026*");

            if (resultadoIdentity.Succeeded)
            {
                await _userManager.AddToRoleAsync(nuevoUsuario, "Veterinario");
                var vete = new Veterinario
                {
                    Nombre = nombre,
                    Especialidad = especialidad,
                    UsuarioId = nuevoUsuario.Id,
                    EstaActivo = true
                };
                _context.Veterinarios.Add(vete);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Veterinario registrado. Login: {emailAcceso}";
                return RedirectToAction(nameof(ListaVeterinarios));
            }
        }
        return RedirectToAction(nameof(ListaVeterinarios));
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstadoVeterinario(int id)
    {
        var vet = await _context.Veterinarios.FindAsync(id);
        if (vet != null)
        {
            vet.EstaActivo = !vet.EstaActivo;
            _context.Update(vet);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ListaVeterinarios));
    }


    public async Task<IActionResult> PacientesPorVolver()
    {
        // Buscamos historias clínicas que tengan una fecha sugerida
        var sugerencias = await _context.HistoriasClinicas
            .Include(h => h.Mascota)
                .ThenInclude(m => m.Usuario)
            .Where(h => h.ProximaCitaSugerida != null)
            .OrderBy(h => h.ProximaCitaSugerida)
            .ToListAsync();

        return View(sugerencias);
    }
    public async Task<IActionResult> PrepararCitaSeguimiento(int mascotaId, string fechaSugerida)
    {
        // Cargamos la mascota con su Usuario para obtener el ID del dueño
        var mascota = await _context.Mascotas.Include(m => m.Usuario).FirstOrDefaultAsync(m => m.Id == mascotaId);
        if (mascota == null) return RedirectToAction(nameof(PacientesPorVolver));

        ViewBag.MascotaId = mascotaId;
        ViewBag.NombreMascota = mascota.Nombre;
        ViewBag.FechaSugerida = fechaSugerida;
        ViewBag.UsuarioId = mascota.UsuarioId; // <--- DATO VITAL PARA EVITAR EL ERROR SQL

        ViewBag.Veterinarios = new SelectList(await _context.Veterinarios.Where(v => v.EstaActivo).ToListAsync(), "Id", "Nombre");

        return View("CrearCitaSeguimiento");
    }

    [HttpPost]
    public async Task<IActionResult> GuardarCitaSeguimiento(Cita cita)
    {
        // 1. Buscamos el médico en la BD usando el ID seleccionado en el formulario
        var veterinario = await _context.Veterinarios.FindAsync(cita.VeterinarioId);

        if (veterinario != null)
        {
            // ASIGNACIÓN CLAVE: Si no haces esto, saldrá "Pendiente" en la tabla
            cita.NombreVeterinario = veterinario.Nombre;
            cita.Estado = "Confirmada"; // Se confirma automáticamente al ser seguimiento
        }

        // 2. Guardamos la nueva cita con todos sus datos (incluyendo el UsuarioId que ya corregimos)
        _context.Citas.Add(cita);

        // 3. Limpiamos el seguimiento de la lista "Pacientes por Volver"
        var historiaAnterior = await _context.HistoriasClinicas
            .Where(h => h.MascotaId == cita.MascotaId && h.ProximaCitaSugerida != null)
            .OrderByDescending(h => h.FechaAtencion)
            .FirstOrDefaultAsync();

        if (historiaAnterior != null)
        {
            historiaAnterior.ProximaCitaSugerida = null;
            _context.Update(historiaAnterior);
        }

        await _context.SaveChangesAsync();

        // Usamos TempData para el mensaje de éxito una sola vez
        TempData["Mensaje"] = "Cita agendada correctamente con el " + cita.NombreVeterinario;

        return RedirectToAction(nameof(GestionCitas));
    }

}