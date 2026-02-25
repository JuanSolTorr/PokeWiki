namespace PokeWiki.Web.Data.Views
{
    public class PokemonBiologiaView
    {
        public int id { get; set; }
        public string nombre { get; set; } = null!;
        public string generacion { get; set; } = null!;
        public string especie { get; set; } = null!;
        public double altura_m { get; set; }
        public double peso_kg { get; set; }
        public string tipos { get; set; } = null!;
        public string habilidades { get; set; } = null!;
        public int hp { get; set; }
        public int atk { get; set; }
        public int def { get; set; }
        public int spatk { get; set; }
        public int spdef { get; set; }
        public int speed { get; set; }
        public string color { get; set; } = null!;
        public string forma_cuerpo { get; set; } = null!;
    }
}