using NugetPokeWiki.DTOs;
using System.Net.Http.Json;
using static NugetPokeWiki.DTOs.ForumCommentDto;

namespace PokeWiki.Web.ApiClients
{
    public class ForumApiClient
    {
        private readonly HttpClient _httpClient;

        public ForumApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<ForumCommentDto>?> GetCommentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ForumCommentDto>>("api/forum");
        }

        public async Task<bool> AddCommentAsync(CreateForumCommentDto newComment)
        {
            var response = await _httpClient.PostAsJsonAsync("api/forum", newComment);
            return response.IsSuccessStatusCode;
        }
    }
}