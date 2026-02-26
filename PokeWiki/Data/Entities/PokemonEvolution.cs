using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokeWiki.Data.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;

    namespace PokeWiki.Web.Data.Entities
    {
        [Table("pokemon_evolution")]
        public class PokemonEvolution
        {
            public int id { get; set; }
            public long? evolved_species_id { get; set; }
            public int? evolution_trigger_id { get; set; }
            public double? trigger_item_id { get; set; }
            public double? minimum_level { get; set; }
            public double? minimum_happiness { get; set; }
            public double? held_item_id { get; set; }
            public double? known_move_id { get; set; }
        }
    }
}
