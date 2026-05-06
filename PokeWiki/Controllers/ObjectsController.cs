using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.ApiClients;
using NugetPokeWiki.DTOs;

namespace PokeWiki.Web.Controllers
{
    public class ObjectsController : Controller
    {
        private static readonly string[] _categorias = ["Evolution", "Battle", "Berries", "Healing", "Poké Balls", "Key Items"];
        private readonly ObjectsApiClient _apiClient;

        public ObjectsController(ObjectsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? categoria, string? search, int page = 1)
        {
            var response = await _apiClient.GetObjectsPageAsync(categoria, search, page);

            ViewData["Section"] = "Items";
            ViewData["CategoriaActual"] = categoria;
            ViewData["Categorias"] = _categorias;
            ViewData["CurrentSearch"] = search;

            if (response != null)
            {
                ViewBag.PaginaActual = response.PaginaActual;
                ViewBag.TotalPaginas = response.TotalPaginas;
                ViewBag.TotalRegistros = response.TotalRegistros;
            }

            var objetos = response?.Items ?? new List<ObjetoDto>();

            if (IsAjaxRequest())
            {
                return PartialView("_ObjectsIndexContent", objetos);
            }

            return View(objetos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var objeto = await _apiClient.GetObjectDetailAsync(id);
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