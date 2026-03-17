using Microsoft.EntityFrameworkCore;
using PokeWiki.Web.Data;
using PokeWiki.Web.Data.Views;
using PokeWiki.Web.Models.ViewModels;

namespace PokeWiki.Web.Repositories
{
    public class RepositoryMoves
    {
        private readonly ApplicationDbContext _context;

        public RepositoryMoves(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovimientosView>> GetMovesAsync(string? search)
        {
            var query = _context.VistaMovimientos
                .Where(m => !string.IsNullOrWhiteSpace(m.mt_numero))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    (m.movimiento != null && m.movimiento.Contains(search)) ||
                    m.mt_numero.Contains(search));
            }

            var rawList = await query.ToListAsync();

            return rawList
                .GroupBy(m => m.mt_numero.Trim().ToUpper())
                .Select(g => g.OrderByDescending(x => x.id).First())
                .OrderBy(m => m.mt_numero.Length)
                .ThenBy(m => m.mt_numero)
                .ToList();
        }

        public async Task<MovimientosView?> FindMoveAsync(int id)
        {
            return await _context.VistaMovimientos
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.id == id);
        }

        public async Task<List<PokemonCompatibleMoveVM>> GetCompatiblePokemonAsync(int moveId)
        {
            return await (from pm in _context.PokemonMoves
                          join p in _context.VistaPokemonBiologia on pm.pokemon_id equals p.id
                          join method in _context.PokemonMoveMethods on pm.pokemon_move_method_id equals method.id
                          where pm.move_id == moveId && method.identifier == "machine"
                          select new PokemonCompatibleMoveVM
                          {
                              Id = p.id,
                              Nombre = p.nombre
                          })
                          .Distinct()
                          .OrderBy(p => p.Id)
                          .ToListAsync();
        }
    }
}
