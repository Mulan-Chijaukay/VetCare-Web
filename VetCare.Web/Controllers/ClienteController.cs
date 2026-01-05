
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Fundamental para identificar al usuario
using VetCare.Web.Data;
using VetCare.Web.Models;

namespace VetCare.Web.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cliente/Inicio
        public async Task<IActionResult> Inicio()
        {
            // Obtener el ID del usuario actual para filtrar sus mascotas
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Solo traemos las mascotas que pertenecen al usuario logueado
            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == userId)
                .ToListAsync();

            return View(mascotas);
        }







        // Método para mostrar la página del formulario
        public IActionResult AgregarMascota()
        {
            return View();
        }

        // Método para procesar el guardado (lo que explicamos antes)
        [HttpPost]
        public async Task<IActionResult> RegistrarMascota(Mascota nuevaMascota)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                nuevaMascota.UsuarioId = userId; // Vincula la mascota al dueño logueado
                _context.Mascotas.Add(nuevaMascota);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Mascota agregada con éxito.";
                return RedirectToAction("Inicio");
            }
            return RedirectToAction("Index", "Account");
        }





        public IActionResult MisMascotas()
        {
            // Obtenemos el reclamo del ID de usuario
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Guardamos el ID como string para la comparación
            string userIdString = userIdClaim.Value;

            // Filtramos comparando strings para evitar el error CS0019
            var mascotas = _context.Mascotas
                                   .Where(m => m.UsuarioId.ToString() == userIdString)
                                   .ToList();

            return View(mascotas);
        }


















        // GET: Cliente/Solicitar
        public async Task<IActionResult> Solicitar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Cargamos solo las mascotas del usuario actual para el menú desplegable
            ViewBag.Mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == userId)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GuardarCita(Cita nuevaCita)
        {
            // 1. Validar campos obligatorios para evitar el SqlException
            if (string.IsNullOrEmpty(nuevaCita.Horario) || string.IsNullOrEmpty(nuevaCita.Servicio))
            {
                TempData["Error"] = "Debes seleccionar un servicio y un horario.";
                return RedirectToAction("Solicitar");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                // 2. Asignación manual de datos faltantes para la DB
                nuevaCita.UsuarioId = userId;
                nuevaCita.Estado = "Pendiente";
                nuevaCita.EsEmergencia = false;

                _context.Citas.Add(nuevaCita);
                await _context.SaveChangesAsync(); // Guarda físicamente en SQL

                TempData["Mensaje"] = "Tu cita ha sido registrada con éxito.";
                return RedirectToAction("Solicitar");
            }

            return RedirectToAction("Index", "Account");
        }
    }
}


