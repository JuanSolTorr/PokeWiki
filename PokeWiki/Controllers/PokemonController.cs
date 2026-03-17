using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.Repositories;

namespace PokeWiki.Web.Controllers
{
    public class PokemonController : Controller
    {
        private readonly RepositoryPokemon _repository;

        public PokemonController(RepositoryPokemon repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int registrosPorPagina = 24;

            var (pokemon, totalRegistros, totalPaginas, currentPage) = await _repository.GetPokemonPageAsync(search, page, registrosPorPagina);

            if (!string.IsNullOrWhiteSpace(search))
            {
                ViewData["CurrentSearch"] = search;
            }

            ViewBag.PaginaActual = currentPage;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            if (IsAjaxRequest())
            {
                return PartialView("_PokemonIndexContent", pokemon);
            }

            return View(pokemon);
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _repository.GetPokemonDetailsAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
                   && requestedWith == "XMLHttpRequest";
        }
    }
}