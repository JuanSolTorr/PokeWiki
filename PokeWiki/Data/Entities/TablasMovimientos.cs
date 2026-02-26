using System.ComponentModel.DataAnnotations.Schema;

namespace PokeWiki.Web.Data.Entities
{
    [Table("pokemon_moves")]
    public class PokemonMove
    {
        public int pokemon_id { get; set; }
        public int move_id { get; set; }
        public int pokemon_move_method_id { get; set; }
        public long? level { get; set; }
    }

    [Table("pokemon_move_methods")]
    public class PokemonMoveMethod
    {
        public int id { get; set; }
        public string? identifier { get; set; }
    }
}