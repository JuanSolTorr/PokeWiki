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

        public async Task<IActionResult> Index()
        {
            // Extraer el universo completo de Pokémon de la Vista SQL
            var datos = await _context.VistaPokemonBiologia
                .OrderBy(p => p.id)
                .Select(p => new PokemonListVM
                {
                    Id = p.id,
                    Nombre = p.nombre,
                    ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{p.id}.png",
                    Tipos = p.tipos.Split('/').Select(t => t.Trim()).ToList(),
                    Generacion = p.generacion
                })
                .ToListAsync();

            ViewData["Section"] = "Archivo Nacional";
            return View(datos);
        }
    }
}