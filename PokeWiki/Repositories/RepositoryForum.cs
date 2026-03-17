using PokeWiki.Web.Models.ViewModels;

namespace PokeWiki.Web.Repositories
{
    public class RepositoryForum
    {
        private static readonly List<ForumCommentVM> _comments = new();
        private static readonly object _sync = new();

        public List<ForumCommentVM> GetOrderedComments()
        {
            lock (_sync)
            {
                return _comments
                    .OrderByDescending(c => c.PublishedAt)
                    .Select(c => new ForumCommentVM
                    {
                        UserName = c.UserName,
                        PublishedAt = c.PublishedAt,
                        Message = c.Message
                    })
                    .ToList();
            }
        }

        public ForumIndexVM BuildModel()
        {
            return new ForumIndexVM
            {
                Comments = GetOrderedComments()
            };
        }

        public void AddComment(string userName, string message)
        {
            lock (_sync)
            {
                _comments.Add(new ForumCommentVM
                {
                    UserName = userName,
                    PublishedAt = DateTime.Now,
                    Message = message.Trim()
                });
            }
        }
    }
}
