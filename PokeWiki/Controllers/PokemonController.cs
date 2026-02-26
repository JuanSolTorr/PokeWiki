using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeWiki.Web.Data;
using PokeWiki.Web.Models.ViewModels;

namespace PokeWiki.Web.Controllers
{
    public class PokemonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PokemonController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int registrosPorPagina = 24;
            var query = _context.VistaPokemonBiologia.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.nombre.Contains(search));
                ViewData["CurrentSearch"] = search;
            }

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

            var datosSql = await query
                .OrderBy(p => p.id)
                .Skip((page - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            var pokemonList = datosSql.Select(p => new PokemonListVM
            {
                Id = p.id,
                Nombre = p.nombre,
                ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{p.id}.png",
                Tipos = p.tipos != null
                            ? p.tipos.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
                            : new List<string>(),
                Generacion = p.generacion
            }).ToList();

            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = totalPaginas == 0 ? 1 : totalPaginas;

            ViewBag.TotalRegistros = totalRegistros;

            return View(pokemonList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var p = await _context.VistaPokemonBiologia.FirstOrDefaultAsync(x => x.id == id);
            if (p == null) return NotFound();

            var vm = new PokemonDetailsVM
            {
                Id = p.id,
                Nombre = p.nombre ?? "DESCONOCIDO",
                Generacion = p.generacion ?? "-",
                Especie = p.especie ?? "-",
                AlturaM = p.altura_m,
                PesoKg = p.peso_kg,
                XpBase = p.xp_base,
                ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{p.id}.png",
                Tipos = p.tipos != null ? p.tipos.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList() : new(),
                Habilidades = p.habilidades != null ? p.habilidades.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim()).ToList() : new(),
                Hp = (int)(p.hp),
                Atk = (int)(p.atk),
                Def = (int)(p.def),
                SpAtk = (int)(p.spatk),
                SpDef = (int)(p.spdef),
                Speed = (int)(p.speed)
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

                vm.Evoluciones = evosSql.Select(e => {
                    var infoEvo = tablaEvo.FirstOrDefault(x => x.evolved_species_id == e.id);
                    string metodo = "FORMA BASE";

                    if (infoEvo != null)
                    {
                        if (infoEvo.minimum_level > 0) metodo = $"NIVEL {infoEvo.minimum_level}";
                        else if (infoEvo.trigger_item_id > 0) metodo = "USAR OBJETO";
                        else if (infoEvo.minimum_happiness > 0) metodo = "AMISTAD";
                        else if (infoEvo.evolution_trigger_id == 2) metodo = "INTERCAMBIO";
                        else if (infoEvo.known_move_id > 0) metodo = "CONOCER MOV.";
                        else metodo = "EVOLUCIÓN";
                    }

                    return new EvolucionVM
                    {
                        IdPokemon = e.id,
                        Nombre = e.nombre ?? "",
                        ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{e.id}.png",
                        DetallesEvolucion = metodo
                    };
                }).ToList();
            }

            var rawMoves = await (from pm in _context.PokemonMoves
                                  join v in _context.VistaMovimientos on pm.move_id equals v.id
                                  join m in _context.PokemonMoveMethods on pm.pokemon_move_method_id equals m.id
                                  where pm.pokemon_id == id
                                  select new { pm.level, m.identifier, v.movimiento, v.tipo, v.clase_daño, v.power, v.accuracy, v.mt_numero }
                                  ).ToListAsync();

            var uniqueMoves = rawMoves.GroupBy(x => x.movimiento).Select(g => g.First()).ToList();

            vm.MovimientosNivel = uniqueMoves.Where(x => x.identifier == "level-up" || (x.level > 0 && x.identifier != "machine"))
                .Select(x => new MovimientoVM
                {
                    Nombre = x.movimiento ?? "-",
                    Tipo = x.tipo ?? "-",
                    Categoria = x.clase_daño ?? "-",
                    Potencia = (int?)x.power,
                    Precision = (int?)x.accuracy,
                    NivelOMt = $"NV. {x.level}"
                }).OrderBy(x => x.NivelOMt.Length).ThenBy(x => x.NivelOMt).ToList();

            vm.MovimientosMT = uniqueMoves.Where(x => x.identifier == "machine" || !string.IsNullOrWhiteSpace(x.mt_numero))
                .Select(x => new MovimientoVM
                {
                    Nombre = x.movimiento ?? "-",
                    Tipo = x.tipo ?? "-",
                    Categoria = x.clase_daño ?? "-",
                    Potencia = (int?)x.power,
                    Precision = (int?)x.accuracy,
                    NivelOMt = string.IsNullOrWhiteSpace(x.mt_numero) ? "MT" : $"MT {x.mt_numero}"
                }).OrderBy(x => x.NivelOMt).ToList();

            return View(vm);
        }
    }
}