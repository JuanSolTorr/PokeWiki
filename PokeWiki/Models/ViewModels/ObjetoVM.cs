namespace PokeWiki.Web.Models.ViewModels
{
    public class ObjetoVM
    {
        public int Id { get; set; }
        public string Identificador { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Efecto { get; set; } = string.Empty;
        public string Rareza { get; set; } = string.Empty;
        public string Icono { get; set; } = "??";
        public string ImagenUrl { get; set; } = string.Empty;
    }

    public class ObjetoDetalleVM : ObjetoVM
    {
        public List<ObjetoGeneracionVM> DondeSeConsiguePorGeneracion { get; set; } = new();
    }

    public class ObjetoGeneracionVM
    {
        public string Generacion { get; set; } = string.Empty;
        public long IndiceJuego { get; set; }
        public string Juegos { get; set; } = string.Empty;
    }
}
