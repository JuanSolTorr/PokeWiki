using System.ComponentModel.DataAnnotations;

namespace PokeWiki.Web.Models.ViewModels
{
    public class ForumIndexVM
    {
        public List<ForumCommentVM> Comments { get; set; } = new();

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string NewMessage { get; set; } = string.Empty;
    }
}
