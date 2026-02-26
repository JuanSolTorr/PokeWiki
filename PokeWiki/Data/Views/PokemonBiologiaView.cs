namespace PokeWiki.Web.Data.Views
{
    public class PokemonBiologiaView
    {
        public int id { get; set; }
        public string nombre { get; set; } = null!;
        public string generacion { get; set; } = null!;
        public string especie { get; set; } = null!;
        public decimal altura_m { get; set; }
        public decimal peso_kg { get; set; }
        public double xp_base { get; set; }
        public string tipos { get; set; } = null!;
        public string habilidades { get; set; } = null!;
        public long hp { get; set; }
        public long atk { get; set; }
        public long def { get; set; }
        public long spatk { get; set; }
        public long spdef { get; set; }
        public long speed { get; set; }
        public long gender_rate { get; set; }
        public long capture_rate { get; set; }
        public long base_happiness { get; set; }
        public string color { get; set; } = null!;
        public string forma_cuerpo { get; set; } = null!;
        public string habitat { get; set; } = null!;
    }
}