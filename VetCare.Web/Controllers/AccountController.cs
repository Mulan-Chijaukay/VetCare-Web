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
                var nuevoUsuario = new Usuario
                {
                    UserName = Email, 
                    Email = Email,
                    Nombre = Username,
                    Rol = "Cliente" 
                };

                var resultado = await _userManager.CreateAsync(nuevoUsuario, Password);

                if (resultado.Succeeded)
                {
                    await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false);

                    return RedirectToAction("Inicio", "Cliente");
                }
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
            var resultado = await _signInManager.PasswordSignInAsync(Email, Password, false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var usuario = await _userManager.FindByEmailAsync(Email);

                if (usuario.Rol == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("Inicio", "Cliente");
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