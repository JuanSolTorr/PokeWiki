using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.ApiClients;
using NugetPokeWiki.DTOs;

namespace PokeWiki.Web.Controllers
{
    public class PokemonController : Controller
    {
        private readonly PokemonApiClient _apiClient;

        public PokemonController(PokemonApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var response = await _apiClient.GetPokemonPageAsync(search, page);

            if (response == null) return View(new List<PokemonListDto>());

            if (!string.IsNullOrWhiteSpace(search)) ViewData["CurrentSearch"] = search;

            ViewBag.PaginaActual = response.PaginaActual;
            ViewBag.TotalPaginas = response.TotalPaginas;
            ViewBag.TotalRegistros = response.TotalRegistros;

            if (IsAjaxRequest())
            {
                return PartialView("_PokemonIndexContent", response.Items);
            }

            return View(response.Items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pokemon = await _apiClient.GetPokemonDetailsAsync(id);
            if (pokemon == null) return NotFound();

            return View(pokemon);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
                   && requestedWith == "XMLHttpRequest";
        }
    }
}