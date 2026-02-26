namespace PokeWiki.Web.Models.ViewModels
{
    // 1. LA CLASE PRINCIPAL (La que ya tenías, pero con las 3 listas nuevas al final)
    public class PokemonDetailsVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Generacion { get; set; } = null!;
        public string Especie { get; set; } = null!;
        public decimal AlturaM { get; set; }
        public decimal PesoKg { get; set; }
        public double XpBase { get; set; }
        public string ImagenUrl { get; set; } = null!;

        public List<string> Tipos { get; set; } = new();
        public List<string> Habilidades { get; set; } = new();

        public int Hp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int SpAtk { get; set; }
        public int SpDef { get; set; }
        public int Speed { get; set; }

        public List<EvolucionVM> Evoluciones { get; set; } = new();
        public List<MovimientoVM> MovimientosNivel { get; set; } = new();
        public List<MovimientoVM> MovimientosMT { get; set; } = new();
    }

    public class EvolucionVM
    {
        public int IdPokemon { get; set; }
        public string Nombre { get; set; } = null!;
        public string DetallesEvolucion { get; set; } = null!;
        public string ImagenUrl { get; set; } = null!;
    }

    // 3. LA CLASE PARA LOS MOVIMIENTOS (¡Y aquí la otra!)
    public class MovimientoVM
    {
        public string Nombre { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string Categoria { get; set; } = null!; // Físico, Especial o Estado
        public int? Potencia { get; set; }
        public int? Precision { get; set; }
        public string NivelOMt { get; set; } = null!; // Ej: "Nv. 15" o "MT04"
    }
}