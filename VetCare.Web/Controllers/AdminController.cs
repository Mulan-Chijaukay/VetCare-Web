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
        ViewBag.Hoy = await _context.Citas.CountAsync(c => c.Fecha.Date == hoy);
        ViewBag.Pendientes = await _context.Citas.CountAsync(c => c.Estado == "Pendiente");
        ViewBag.Mascotas = await _context.Mascotas.CountAsync();
        return View();
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
}