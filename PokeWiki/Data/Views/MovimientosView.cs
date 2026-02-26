namespace PokeWiki.Web.Data.Views
{
    public class MovimientosView
    {
        public int id { get; set; }
        public string? movimiento { get; set; }
        public string? tipo { get; set; }
        public string? clase_daño { get; set; }
        public double? power { get; set; }
        public double? pp { get; set; }
        public double? accuracy { get; set; }
        public long? priority { get; set; }
        public string? objetivo { get; set; }
        public string mt_numero { get; set; } = null!;
        public int? efecto_id { get; set; }
    }
}