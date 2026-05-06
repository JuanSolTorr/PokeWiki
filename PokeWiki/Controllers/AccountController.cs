using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetPokeWiki.DTOs;
using PokeWiki.Web.ApiClients; // <-- Usamos la nueva carpeta
using System.Security.Claims;

namespace PokeWiki.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthApiClient _apiClient;

        public AccountController(AuthApiClient apiClient)
        {
            _apiClient = apiClient;
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

            var registerDto = new UserRegisterDto { Username = username, Email = email, Password = password };
            var response = await _apiClient.RegisterAsync(registerDto);

            if (response == null)
            {
                ViewBag.Error = "Email is already in use or an error occurred.";
                return View();
            }

            // Guardamos al usuario y su Token
            await SignInUserAsync(response.Username, response.Email, response.Token);
            SetUserSession(response.Username, response.Email);

            return RedirectToAction("Index", "Pokemon");
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var loginDto = new UserLoginDto { Email = email, Password = password };
            var response = await _apiClient.LoginAsync(loginDto);

            if (response == null)
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            await SignInUserAsync(response.Username, response.Email, response.Token);
            SetUserSession(response.Username, response.Email);
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
            if (!TryGetCurrentEmail(out var email)) return RedirectToAction("Login", "Account");

            LoadProfileViewData(email);
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            if (!TryGetCurrentEmail(out var email)) return RedirectToAction("Login");

            if (!IsValidPassword(newPassword))
            {
                ViewBag.PasswordError = "New password must have at least 8 characters.";
                LoadProfileViewData(email);
                return View("Profile");
            }

            // Rescatamos el Token que guardamos en la cookie
            var token = User.FindFirstValue("jwt_token") ?? string.Empty;

            var dto = new ChangePasswordDto { CurrentPassword = currentPassword, NewPassword = newPassword };
            bool success = await _apiClient.ChangePasswordAsync(dto, token);

            if (!success) ViewBag.PasswordError = "Current password is incorrect or an error occurred.";
            else ViewBag.PasswordSuccess = "Password updated successfully.";

            LoadProfileViewData(email);
            return View("Profile");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string confirmPassword)
        {
            if (!TryGetCurrentEmail(out var email)) return RedirectToAction("Login");

            var token = User.FindFirstValue("jwt_token") ?? string.Empty;
            bool success = await _apiClient.DeleteAccountAsync(confirmPassword, token);

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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeTheme(string theme)
        {
            Response.Cookies.Append("theme", theme, new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });
            return RedirectToAction("Profile");
        }

        private async Task SignInUserAsync(string userName, string email, string jwtToken)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userName),
                new(ClaimTypes.Email, email),
                new("jwt_token", jwtToken) // ¡AQUÍ ESTÁ LA MAGIA! Guardamos el JWT en la sesión del MVC[cite: 4]
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });
        }

        private void SetUserSession(string userName, string email)
        {
            HttpContext.Session.SetString("User", userName);
            HttpContext.Session.SetString("Email", email);
        }

        private bool TryGetCurrentEmail(out string email)
        {
            email = User.FindFirstValue(ClaimTypes.Email) ?? HttpContext.Session.GetString("Email") ?? string.Empty;
            return !string.IsNullOrWhiteSpace(email);
        }

        private void LoadProfileViewData(string email)
        {
            ViewData["UserName"] = User.Identity?.Name ?? HttpContext.Session.GetString("User");
            ViewData["Email"] = email;
        }

        private static bool IsValidPassword(string password) => !string.IsNullOrWhiteSpace(password) && password.Length >= 8;
    }
}