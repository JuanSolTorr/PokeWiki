using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.Data.Entities;
using PokeWiki.Web.Repositories;

namespace PokeWiki.Web.Controllers
{
    public class AccountController : Controller
    {
        private RepositoryUsuarios _repo;

        public AccountController(RepositoryUsuarios repo)
        {
            this._repo = repo;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            await _repo.RegisterUserAsync(username, email, password);
            HttpContext.Session.SetString("User", username);

            return RedirectToAction("Index", "Pokemon");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            Usuario? user = await _repo.LogInUserAsync(email, password);

            if (user == null)
            {
                ViewBag.Error = "Credenciales incorrectas.";
                return View();
            }

            HttpContext.Session.SetString("User", user.Username);
            return RedirectToAction("Index", "Pokemon");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}