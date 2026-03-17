using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.Models.ViewModels;
using PokeWiki.Web.Repositories;

namespace PokeWiki.Web.Controllers
{
    public class ForumController : Controller
    {
        private readonly RepositoryForum _repository;

        public ForumController(RepositoryForum repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = _repository.BuildModel();
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
                if (IsAjaxRequest())
                {
                    return BadRequest();
                }

                ViewData["Section"] = "Community";
                model.Comments = _repository.GetOrderedComments();
                return View("Index", model);
            }

            _repository.AddComment(userName, model.NewMessage);

            if (IsAjaxRequest())
            {
                var comments = _repository.GetOrderedComments();
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
