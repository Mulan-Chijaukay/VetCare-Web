using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VetCare.Web.Models;

namespace VetCare.Web.Controllers
{


    public class AccountController : Controller
    {
        //  SignInManager y UserManager se usara dado q son las herramientas oficiales de Identity
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;


        public AccountController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index() => View();


        [HttpPost]
        public async Task<IActionResult> Register(string Username, string Email, string Password)
        {
            if (ModelState.IsValid)
            {
                // 1. Validar que el correo no exista
                var usuarioExistente = await _userManager.FindByEmailAsync(Email);
                if (usuarioExistente != null)
                {
                    TempData["ErrorRegistro"] = "• Este correo ya está registrado. Intenta iniciar sesión.";
                    return View("Index");
                }

                // 2. Crear el objeto de usuario
                var nuevoUsuario = new Usuario
                {
                    UserName = Email,
                    Email = Email,
                    Nombre = Username, // Aquí guardamos el nombre completo
                    Rol = "Cliente"
                };

                var resultado = await _userManager.CreateAsync(nuevoUsuario, Password);

                if (resultado.Succeeded)
                {
                    await _userManager.AddToRoleAsync(nuevoUsuario, "Cliente");
                    await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false);
                    return RedirectToAction("Inicio", "Cliente");
                }

                // 3. Capturar errores de contraseña de Identity (longitud, números, etc.)
                string listaErrores = string.Join("<br>", resultado.Errors.Select(e => "• " + e.Description));
                TempData["ErrorRegistro"] = listaErrores;
            }
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            // Cambiamos lockoutOnFailure a true para mayor seguridad
            var resultado = await _signInManager.PasswordSignInAsync(Email, Password, false, lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                var usuario = await _userManager.FindByEmailAsync(Email);
                if (await _userManager.IsInRoleAsync(usuario, "Admin")) return RedirectToAction("Dashboard", "Admin");
                if (await _userManager.IsInRoleAsync(usuario, "Veterinario")) return RedirectToAction("Inicio", "Veterinario");
                return RedirectToAction("Inicio", "Cliente");
            }

            // Manejo de errores
            if (resultado.IsLockedOut)
                TempData["ErrorLogin"] = "Cuenta bloqueada por seguridad. Intenta más tarde.";
            else
                TempData["ErrorLogin"] = "El correo o la contraseña no coinciden.";

            return View("Index");
        }


        // Acción para la página de información de la empresa
        public IActionResult QuienesSomos()
        {
            return View();
        }

        // Acción para las promociones dinámicas
        public IActionResult Ofertas()
        {
            // Detectamos si es sábado para resaltar el "Sábado de Spa"
            ViewBag.HoyEsSabado = DateTime.Today.DayOfWeek == DayOfWeek.Saturday;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // Esto limpia las cabeceras de caché solo para esta respuesta
            HttpContext.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            HttpContext.Response.Headers.Append("Pragma", "no-cache");
            HttpContext.Response.Headers.Append("Expires", "0");

            return RedirectToAction("Index", "Account");
        }
    }
}