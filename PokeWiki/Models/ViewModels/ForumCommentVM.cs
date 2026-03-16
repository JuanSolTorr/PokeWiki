using System.ComponentModel.DataAnnotations;

namespace PokeWiki.Web.Models.ViewModels
{
    public class ForumCommentVM
    {
        public string UserName { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres.")]
        public string Message { get; set; } = string.Empty;
    }
}
