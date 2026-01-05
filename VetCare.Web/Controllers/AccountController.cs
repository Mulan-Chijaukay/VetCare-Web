

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Web.Data;
using VetCare.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BCrypt.Net;

namespace VetCare.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string Username, string Email, string Password)
        {
            if (ModelState.IsValid)
            {
                
                var nuevoUsuario = new Usuario
                {
                    UsuarioId = Guid.NewGuid().ToString(),
                    Nombre = Username,
                    Email = Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(Password) 
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == Email);

            if (usuario != null && BCrypt.Net.BCrypt.Verify(Password, usuario.Password))
            {
                // IMPORTANTE: Para que User.FindFirstValue funcione en el resto del sitio,
                // debemos crear la cookie de autenticación con el ID del usuario
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId), // El GUID string
                    new Claim(ClaimTypes.Email, usuario.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Inicio", "Cliente");
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            return View("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}