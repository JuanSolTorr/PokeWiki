using System.ComponentModel.DataAnnotations;

namespace PokeWiki.Web.Models.ViewModels
{
    public class ForumCommentVM
    {
        public string UserName { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}
