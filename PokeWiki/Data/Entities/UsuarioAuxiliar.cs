using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokeWiki.Web.Data.Entities
{
    [Table("UsuarioAuxiliar")]
    public class UsuarioAuxiliar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UsuarioId { get; set; }
        public string Contrasenia_Hasheada { get; set; } = null!;
        public string Salt { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }

}