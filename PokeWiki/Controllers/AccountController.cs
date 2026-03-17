using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.Data.Entities;
using PokeWiki.Web.Repositories;
using System.Security.Claims;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            if (!IsValidPassword(password))
            {
                ViewBag.Error = "Password must have at least 8 characters.";
                return View();
            }

            if (await _repo.ExistsEmailAsync(email))
            {
                ViewBag.Error = "Email is already in use.";
                return View();
            }

            await _repo.RegisterUserAsync(username, email, password);
            await SignInUserAsync(username, email);
            SetUserSession(username, email);

            return RedirectToAction("Index", "Pokemon");
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            Usuario? user = await _repo.LogInUserAsync(email, password);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            await SignInUserAsync(user.Username, user.Email);
            SetUserSession(user.Username, user.Email);
            return RedirectToAction("Index", "Pokemon");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public IActionResult Profile()
        {
            if (!TryGetCurrentEmail(out var email))
            {
                return RedirectToAction("Login", "Account");
            }

            LoadProfileViewData(email);
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeTheme(string theme)
        {
            Response.Cookies.Append("theme", theme, new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            if (!TryGetCurrentEmail(out var email))
            {
                return RedirectToAction("Login");
            }

            if (!IsValidPassword(newPassword))
            {
                ViewBag.PasswordError = "New password must have at least 8 characters.";
                LoadProfileViewData(email);
                return View("Profile");
            }

            bool success = await _repo.ChangePasswordAsync(email, currentPassword, newPassword);

            if (!success)
            {
                ViewBag.PasswordError = "Current password is incorrect or an error occurred.";
            }
            else
            {
                ViewBag.PasswordSuccess = "Password updated successfully.";
            }

            LoadProfileViewData(email);
            return View("Profile");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string confirmPassword)
        {
            if (!TryGetCurrentEmail(out var email))
            {
                return RedirectToAction("Login");
            }

            bool success = await _repo.DeleteAccountAsync(email, confirmPassword);

            if (!success)
            {
                ViewBag.DeleteError = "Incorrect password. Account could not be deleted.";
                LoadProfileViewData(email);
                return View("Profile");
            }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Pokemon");
        }

        private async Task SignInUserAsync(string userName, string email)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userName),
                new(ClaimTypes.Email, email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });
        }

        private void SetUserSession(string userName, string email)
        {
            HttpContext.Session.SetString("User", userName);
            HttpContext.Session.SetString("Email", email);
        }

        private bool TryGetCurrentEmail(out string email)
        {
            email = User.FindFirstValue(ClaimTypes.Email)
                ?? HttpContext.Session.GetString("Email")
                ?? string.Empty;

            return !string.IsNullOrWhiteSpace(email);
        }

        private void LoadProfileViewData(string email)
        {
            ViewData["UserName"] = User.Identity?.Name ?? HttpContext.Session.GetString("User");
            ViewData["Email"] = email;
        }

        private static bool IsValidPassword(string password)
            => !string.IsNullOrWhiteSpace(password) && password.Length >= 8;
    }
}