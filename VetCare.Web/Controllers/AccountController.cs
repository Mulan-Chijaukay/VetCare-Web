using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VetCare.Web.Models;

namespace VetCare.Web.Controllers
{
    public class AccountController : Controller
    {
        // Usamos SignInManager y UserManager que son las herramientas oficiales de Identity
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
                // 1. Creamos el objeto Usuario con los datos del formulario
                var nuevoUsuario = new Usuario
                {
                    UserName = Email, // Identity usa UserName para loguearse, usamos el Email
                    Email = Email,
                    Nombre = Username,
                    Rol = "Cliente" // Asignamos el rol por defecto
                };

                // 2. IMPORTANTE: Usamos _userManager para crearlo. 
                // Esto encripta la contraseña y llena los campos Normalized y SecurityStamp.
                var resultado = await _userManager.CreateAsync(nuevoUsuario, Password);

                if (resultado.Succeeded)
                {
                    // 3. Si se creó bien, lo logueamos automáticamente
                    await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false);

                    // 4. Lo mandamos a su página de inicio
                    return RedirectToAction("Inicio", "Cliente");
                }

                // Si hay errores (ej: contraseña muy corta), los mostramos
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            // 1. Validar credenciales y crear cookie
            var resultado = await _signInManager.PasswordSignInAsync(Email, Password, false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                // 2. Buscar al usuario para ver su rol
                var usuario = await _userManager.FindByEmailAsync(Email);

                // 3. Redirección inteligente según el rol guardado en la DB
                if (usuario.Rol == "Admin")
                {
                    return RedirectToAction("Index", "Admin"); // Si tienes un panel admin
                }

                return RedirectToAction("Inicio", "Cliente"); // Panel de cliente
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            return View("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}