using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.ApiClients;

namespace PokeWiki.Web.Controllers
{
    public class MovesController : Controller
    {
        private readonly MovesApiClient _apiClient;

        public MovesController(MovesApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var moves = await _apiClient.GetMovesAsync(search);
            ViewData["CurrentSearch"] = search;

            if (IsAjaxRequest())
            {
                return PartialView("_MovesIndexContent", moves);
            }

            return View(moves);
        }

        public async Task<IActionResult> Details(int id)
        {
            var move = await _apiClient.GetMoveAsync(id);
            if (move == null)
            {
                return NotFound();
            }

            ViewBag.PokemonCompatibles = await _apiClient.GetCompatiblePokemonAsync(id);
            return View(move);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
                   && requestedWith == "XMLHttpRequest";
        }
    }
}