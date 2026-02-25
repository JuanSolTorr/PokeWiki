// Archivo: Models/ViewModels/PokemonListVM.cs
namespace PokeWiki.Web.Models.ViewModels
{
    public class PokemonListVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string ImagenUrl { get; set; } = null!;
        public List<string> Tipos { get; set; } = new();
        public string Generacion { get; set; } = null!;
    }
}