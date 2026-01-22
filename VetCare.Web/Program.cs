using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; // Necesario para ResponseCache
using Microsoft.EntityFrameworkCore;
using VetCare.Web.Data;
using VetCare.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Conexión a la Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// 2. CONFIGURACIÓN DE IDENTITY REFORZADA
builder.Services.AddIdentity<Usuario, IdentityRole>(options => {
    // REGLAS 
    options.Password.RequireDigit = true;           // Al menos un número
    options.Password.RequiredLength = 8;            // Mínimo 8 caracteres
    options.Password.RequireNonAlphanumeric = false; // DESACTIVADO: No obliga a usar símbolos (@#$)
    options.Password.RequireUppercase = true;       // Una Mayúscula
    options.Password.RequireLowercase = true;       // Una Minúscula

    // Bloqueo de cuenta tras 5 intentos fallidos
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. CONFIGURACIÓN DE COOKIES Y CACHÉ
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Index";
    // CAMBIO AQUÍ: Redirigimos al Login en lugar de AccessDenied
    options.AccessDeniedPath = "/Account/Index";

    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true; // Seguridad extra
});

// 4. CONFIGURACIÓN ESTÁNDAR (Sin el filtro global que causa bucles)
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var app = builder.Build();

// Pipeline de configuración
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// EL ORDEN AQUÍ ES VITAL:
app.UseAuthentication(); // 1. Quién eres
app.UseAuthorization();  // 2. Qué puedes hacer

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=QuienesSomos}/{id?}");
app.MapRazorPages();

// Bloque de inicialización de roles y Admin (Tu lógica actual)
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();

    string[] roleNames = { "Admin", "Veterinario", "Cliente" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminEmail = "admin@vetcare.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new Usuario
        {
            UserName = adminEmail,
            Email = adminEmail,
            Nombre = "Administrador Principal",
            Rol = "Admin",
            EmailConfirmed = true
        };

        // NOTA: Cambia "Admin123!" por algo que cumpla tus nuevas reglas
        // (Ya tiene Mayúscula, minúscula, número y símbolo, así que está bien)
        var result = await userManager.CreateAsync(newAdmin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}

app.Run();