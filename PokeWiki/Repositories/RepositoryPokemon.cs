using Microsoft.EntityFrameworkCore;
using PokeWiki.Web.Data;
using PokeWiki.Web.Models.ViewModels;

namespace PokeWiki.Web.Repositories
{
    public class RepositoryPokemon
    {
        private readonly ApplicationDbContext _context;

        public RepositoryPokemon(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<PokemonListVM> Pokemon, int TotalRegistros, int TotalPaginas, int CurrentPage)> GetPokemonPageAsync(string? search, int page, int pageSize)
        {
            var query = _context.VistaPokemonBiologia.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.nombre.Contains(search));
            }

            var totalRegistros = await query.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)pageSize));

            if (page < 1)
            {
                page = 1;
            }

            if (page > totalPaginas)
            {
                page = totalPaginas;
            }

            var datosSql = await query
                .OrderBy(p => p.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pokemon = datosSql.Select(p => new PokemonListVM
            {
                Id = p.id,
                Nombre = p.nombre,
                ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{p.id}.png",
                Tipos = p.tipos != null
                    ? p.tipos.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
                    : new List<string>(),
                Generacion = p.generacion
            }).ToList();

            return (pokemon, totalRegistros, totalPaginas, page);
        }

        public async Task<PokemonDetailsVM?> GetPokemonDetailsAsync(int id)
        {
            var p = await _context.VistaPokemonBiologia.FirstOrDefaultAsync(x => x.id == id);
            if (p == null)
            {
                return null;
            }

            var vm = new PokemonDetailsVM
            {
                Id = p.id,
                Nombre = p.nombre ?? "UNKNOWN",
                Generacion = p.generacion ?? "-",
                Especie = p.especie ?? "-",
                AlturaM = p.altura_m,
                PesoKg = p.peso_kg,
                XpBase = p.xp_base,
                ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{p.id}.png",
                Tipos = p.tipos != null ? p.tipos.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList() : new(),
                Habilidades = p.habilidades != null ? p.habilidades.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim()).ToList() : new(),
                Hp = (int)p.hp,
                Atk = (int)p.atk,
                Def = (int)p.def,
                SpAtk = (int)p.spatk,
                SpDef = (int)p.spdef,
                Speed = (int)p.speed
            };

            var datosFamilia = await _context.VistaEvolucionCrianza.FirstOrDefaultAsync(x => x.especie_id == id);
            if (datosFamilia != null)
            {
                var idsFamilia = await _context.VistaEvolucionCrianza
                    .Where(x => x.id_cadena_evolutiva == datosFamilia.id_cadena_evolutiva)
                    .Select(x => x.especie_id)
                    .ToListAsync();

                var evosSql = await _context.VistaPokemonBiologia
                    .Where(x => idsFamilia.Contains(x.id))
                    .OrderBy(x => x.id)
                    .ToListAsync();

                var tablaEvo = await _context.PokemonEvolutions
                    .Where(x => idsFamilia.Contains((int)(x.evolved_species_id ?? 0)))
                    .ToListAsync();

                vm.Evoluciones = evosSql.Select(e =>
                {
                    var infoEvo = tablaEvo.FirstOrDefault(x => x.evolved_species_id == e.id);
                    var metodo = "BASE FORM";

                    if (infoEvo != null)
                    {
                        if (infoEvo.minimum_level > 0) metodo = $"LEVEL {infoEvo.minimum_level}";
                        else if (infoEvo.trigger_item_id > 0) metodo = "USE ITEM";
                        else if (infoEvo.minimum_happiness > 0) metodo = "FRIENDSHIP";
                        else if (infoEvo.evolution_trigger_id == 2) metodo = "TRADE";
                        else if (infoEvo.known_move_id > 0) metodo = "KNOW MOVE";
                        else metodo = "EVOLUTION";
                    }

                    return new EvolucionVM
                    {
                        IdPokemon = e.id,
                        Nombre = e.nombre ?? string.Empty,
                        ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{e.id}.png",
                        DetallesEvolucion = metodo
                    };
                }).ToList();
            }

            var rawMoves = await (from pm in _context.PokemonMoves
                                  join v in _context.VistaMovimientos on pm.move_id equals v.id
                                  join m in _context.PokemonMoveMethods on pm.pokemon_move_method_id equals m.id
                                  where pm.pokemon_id == id
                                  select new { pm.level, m.identifier, v.movimiento, v.tipo, v.clase_daño, v.power, v.accuracy, v.mt_numero })
                .ToListAsync();

            var uniqueMoves = rawMoves.GroupBy(x => x.movimiento).Select(g => g.First()).ToList();

            vm.MovimientosNivel = uniqueMoves
                .Where(x => x.identifier == "level-up" || (x.level > 0 && x.identifier != "machine"))
                .Select(x => new MovimientoVM
                {
                    Nombre = x.movimiento ?? "-",
                    Tipo = x.tipo ?? "-",
                    Categoria = x.clase_daño ?? "-",
                    Potencia = (int?)x.power,
                    Precision = (int?)x.accuracy,
                    NivelOMt = $"LV. {x.level}"
                })
                .OrderBy(x => x.NivelOMt.Length)
                .ThenBy(x => x.NivelOMt)
                .ToList();

            vm.MovimientosMT = uniqueMoves
                .Where(x => x.identifier == "machine" || !string.IsNullOrWhiteSpace(x.mt_numero))
                .Select(x => new MovimientoVM
                {
                    Nombre = x.movimiento ?? "-",
                    Tipo = x.tipo ?? "-",
                    Categoria = x.clase_daño ?? "-",
                    Potencia = (int?)x.power,
                    Precision = (int?)x.accuracy,
                    NivelOMt = string.IsNullOrWhiteSpace(x.mt_numero) ? "TM" : $"TM {x.mt_numero}"
                })
                .OrderBy(x => x.NivelOMt)
                .ToList();

            return vm;
        }
    }
}
