using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VetCare.Web.Data;
using VetCare.Web.Models;

namespace VetCare.Web.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public ClienteController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

      
        public async Task<IActionResult> Inicio()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Index", "Account");

            var hoy = DateTime.Today;

            
            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == userId)
                .ToListAsync();

            var citasHoy = await _context.Citas
                .Include(c => c.Mascota)
                .Where(c => c.UsuarioId == userId && c.Fecha.Date == hoy)
                .OrderBy(c => c.Horario)
                .ToListAsync();

            ViewBag.CitasHoy = citasHoy;

            return View(mascotas);
        }

        public IActionResult AgregarMascota() => View();

        [HttpPost]
        public async Task<IActionResult> RegistrarMascota(Mascota nuevaMascota)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                nuevaMascota.UsuarioId = userId;
                _context.Mascotas.Add(nuevaMascota);
                await _context.SaveChangesAsync();
                return RedirectToAction("Inicio");
            }
            return RedirectToAction("Index", "Account");
        }

        public IActionResult MisMascotas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var mascotas = _context.Mascotas
                .Where(m => m.UsuarioId == userId)
                .ToList();

            return View(mascotas);
        }

        public async Task<IActionResult> Solicitar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == userId)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GuardarCita(Cita nuevaCita)
        {
            if (string.IsNullOrEmpty(nuevaCita.Horario) || string.IsNullOrEmpty(nuevaCita.Servicio))
            {
                TempData["Error"] = "Debes seleccionar un servicio y un horario.";
                return RedirectToAction("Solicitar");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                nuevaCita.UsuarioId = userId;
                nuevaCita.Estado = "Pendiente";
                nuevaCita.EsEmergencia = false;

                _context.Citas.Add(nuevaCita);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Tu cita ha sido registrada exitosamente.";
                return RedirectToAction("Solicitar");
            }
            return RedirectToAction("Index", "Account");
        }

        public IActionResult Citas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var citas = _context.Citas
                .Include(c => c.Mascota)
                .Where(c => c.UsuarioId == userId)
                .OrderByDescending(c => c.Fecha)
                .ToList();

            return View(citas);
        }

        public async Task<IActionResult> Historial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var historial = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.Veterinario)
                .Where(c => c.UsuarioId == userId &&
                           (c.Estado == "Completada" || c.Estado == "Cancelada"))
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return View(historial);
        }

        public async Task<IActionResult> Configuracion()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarConfiguracion(string nombre, string telefono)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario != null)
            {
                usuario.Nombre = nombre;
                usuario.PhoneNumber = telefono;

                var resultado = await _userManager.UpdateAsync(usuario);
                if (resultado.Succeeded)
                {
                    TempData["MensajeConfig"] = "Tus datos han sido actualizados con éxito.";
                    return RedirectToAction("Configuracion");
                }
            }

            TempData["Error"] = "No se pudieron guardar los cambios.";
            return RedirectToAction("Configuracion");
        }

        [HttpPost]
        public async Task<IActionResult> CambiarPassword(string currentPassword, string newPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _userManager.FindByIdAsync(userId);

            var resultado = await _userManager.ChangePasswordAsync(usuario, currentPassword, newPassword);

            if (resultado.Succeeded)
            {
                TempData["MensajeConfig"] = "Contraseña actualizada correctamente.";
            }
            else
            {
                TempData["Error"] = "La contraseña actual es incorrecta o la nueva no cumple los requisitos.";
            }
            return RedirectToAction("Configuracion");
        }
    }
}


