using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.Models.ViewModels;

namespace PokeWiki.Web.Controllers
{
    public class ForumController : Controller
    {
        private static readonly List<ForumCommentVM> _comments = new();
        private static readonly object _sync = new();

        [HttpGet]
        public IActionResult Index()
        {
            var model = BuildModel();
            ViewData["Section"] = "Community";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ForumIndexVM model)
        {
            var userName = HttpContext.Session.GetString("User");
            if (string.IsNullOrWhiteSpace(userName))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Section"] = "Community";
                model.Comments = GetOrderedComments();
                return View("Index", model);
            }

            lock (_sync)
            {
                _comments.Add(new ForumCommentVM
                {
                    UserName = userName,
                    PublishedAt = DateTime.Now,
                    Message = model.NewMessage.Trim()
                });
            }

            return RedirectToAction(nameof(Index));
        }

        private static List<ForumCommentVM> GetOrderedComments()
        {
            lock (_sync)
            {
                return _comments
                    .OrderByDescending(c => c.PublishedAt)
                    .Select(c => new ForumCommentVM
                    {
                        UserName = c.UserName,
                        PublishedAt = c.PublishedAt,
                        Message = c.Message
                    })
                    .ToList();
            }
        }

        private ForumIndexVM BuildModel()
        {
            return new ForumIndexVM
            {
                Comments = GetOrderedComments()
            };
        }
    }
}
