using System.ComponentModel.DataAnnotations;

namespace PokeWiki.Web.Models.ViewModels
{
    public class ForumIndexVM
    {
        public List<ForumCommentVM> Comments { get; set; } = new();

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres.")]
        public string NewMessage { get; set; } = string.Empty;
    }
}
