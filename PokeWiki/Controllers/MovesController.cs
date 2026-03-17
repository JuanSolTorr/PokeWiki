using Microsoft.AspNetCore.Mvc;
using PokeWiki.Web.Repositories;

namespace PokeWiki.Web.Controllers
{
    public class MovesController : Controller
    {
        private readonly RepositoryMoves _repository;

        public MovesController(RepositoryMoves repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var moves = await _repository.GetMovesAsync(search);
            ViewData["CurrentSearch"] = search;

            if (IsAjaxRequest())
            {
                return PartialView("_MovesIndexContent", moves);
            }

            return View(moves);
        }

        public async Task<IActionResult> Details(int id)
        {
            var move = await _repository.FindMoveAsync(id);
            if (move == null)
            {
                return NotFound();
            }

            ViewBag.PokemonCompatibles = await _repository.GetCompatiblePokemonAsync(id);
            return View(move);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
                   && requestedWith == "XMLHttpRequest";
        }
    }
}