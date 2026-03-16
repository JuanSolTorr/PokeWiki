using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.Data.Entities;
using PokeWiki.Web.Repositories;

namespace PokeWiki.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly RepositoryUsuarios _repo;

        public AccountController(RepositoryUsuarios repo)
        {
            _repo = repo;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            await _repo.RegisterUserAsync(username, email, password);
            HttpContext.Session.SetString("User", username);
            HttpContext.Session.SetString("Email", email); // Guardamos el email para operaciones de cuenta

            return RedirectToAction("Index", "Pokemon");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            Usuario? user = await _repo.LogInUserAsync(email, password);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            HttpContext.Session.SetString("User", user.Username);
            HttpContext.Session.SetString("Email", user.Email); // Guardamos el email
            
            return RedirectToAction("Index", "Pokemon");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // Nueva acción para la vista del Perfil
        public async Task<IActionResult> Profile()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            // Podrías cargar datos adicionales del usuario desde la BBDD si lo requieres
            ViewData["UserName"] = HttpContext.Session.GetString("User");
            ViewData["Email"] = email;
            
            return View();
        }

        // Acción para gestionar ajustes (Theme, ChangePassword, DeleteAccount)
        [HttpPost]
        public async Task<IActionResult> ChangeTheme(string theme)
        {
            // Implementación de cambio de tema (podría guardarse en cookies o sesión)
            Response.Cookies.Append("theme", theme, new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            bool success = await _repo.ChangePasswordAsync(email, currentPassword, newPassword);
            
            if (!success)
            {
                ViewBag.PasswordError = "Current password is incorrect or an error occurred.";
            }
            else
            {
                ViewBag.PasswordSuccess = "Password updated successfully.";
            }

            return View("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(string confirmPassword)
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            bool success = await _repo.DeleteAccountAsync(email, confirmPassword);
            
            if (!success)
            {
                ViewBag.DeleteError = "Incorrect password. Account could not be deleted.";
                return View("Profile");
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Pokemon");
        }
    }
}