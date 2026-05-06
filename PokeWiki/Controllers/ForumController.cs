using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NugetPokeWiki.DTOs;
using PokeWiki.Web.ApiClients;
using static NugetPokeWiki.DTOs.ForumCommentDto;

namespace PokeWiki.Web.Controllers
{
    public class ForumController : Controller
    {
        private readonly ForumApiClient _apiClient;

        public ForumController(ForumApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var comments = await _apiClient.GetCommentsAsync() ?? new List<ForumCommentDto>();
            ViewData["Section"] = "Community";
            return View(comments); // Actualiza tu vista Index.cshtml para usar @model IEnumerable<ForumCommentDto>
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string NewMessage)
        {
            var userName = HttpContext.Session.GetString("User");
            if (string.IsNullOrWhiteSpace(userName))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(NewMessage))
            {
                if (IsAjaxRequest()) return BadRequest();
                return RedirectToAction(nameof(Index));
            }

            var dto = new CreateForumCommentDto
            {
                UserName = userName,
                Message = NewMessage
            };

            await _apiClient.AddCommentAsync(dto);

            if (IsAjaxRequest())
            {
                var comments = await _apiClient.GetCommentsAsync() ?? new List<ForumCommentDto>();
                return PartialView("_ForumComments", comments);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
                   && requestedWith == "XMLHttpRequest";
        }
    }
}