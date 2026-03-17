using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.Repositories;

namespace PokeWiki.Web.Controllers
{
    public class ObjectsController : Controller
    {
        private static readonly string[] _categorias = ["Evolution", "Battle", "Berries", "Healing", "Poké Balls", "Key Items"];
        private readonly RepositoryObjects _repository;

        public ObjectsController(RepositoryObjects repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(string? categoria, string? search, int page = 1)
        {
            const int pageSize = 24;

            var (objetos, totalRegistros, totalPaginas, currentPage) =
                await _repository.GetObjectsPageAsync(categoria, search, page, pageSize);

            ViewData["Section"] = "Items";
            ViewData["CategoriaActual"] = categoria;
            ViewData["Categorias"] = _categorias;
            ViewData["CurrentSearch"] = search;
            ViewBag.PaginaActual = currentPage;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            if (IsAjaxRequest())
            {
                return PartialView("_ObjectsIndexContent", objetos);
            }

            return View(objetos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var objeto = await _repository.GetObjectDetailAsync(id);
            if (objeto is null)
            {
                return NotFound();
            }

            ViewData["Section"] = "Items";
            return View(objeto);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
                   && requestedWith == "XMLHttpRequest";
        }
    }
}
