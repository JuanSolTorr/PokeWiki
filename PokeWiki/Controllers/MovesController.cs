using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeWiki.Web.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PokeWiki.Web.Controllers
{
    public class MovesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            var query = _context.VistaMovimientos
                .Where(m => m.mt_numero != null && m.mt_numero != "")
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.movimiento.Contains(search) || m.mt_numero.Contains(search));
            }

            var listaBruta = await query.ToListAsync();

            var movimientosUnicos = listaBruta
                .GroupBy(m => m.mt_numero.Trim().ToUpper())
                .Select(g => g.OrderByDescending(x => x.id).First())
                .OrderBy(m => m.mt_numero.Length)
                .ThenBy(m => m.mt_numero)
                .ToList();

            ViewData["CurrentSearch"] = search;
            return View(movimientosUnicos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var movimiento = await _context.VistaMovimientos
                .FirstOrDefaultAsync(m => m.id == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            var pokemonCompatibles = await (from pm in _context.PokemonMoves
                                            join p in _context.VistaPokemonBiologia on pm.pokemon_id equals p.id
                                            join m in _context.PokemonMoveMethods on pm.pokemon_move_method_id equals m.id
                                            where pm.move_id == id && m.identifier == "machine"
                                            select new { p.id, p.nombre })
                                            .Distinct()
                                            .OrderBy(p => p.id)
                                            .ToListAsync();

            ViewBag.PokemonCompatibles = pokemonCompatibles;
            return View(movimiento);
        }
    }
}