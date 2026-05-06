using NugetPokeWiki.DTOs;
using System.Net.Http.Json;

namespace PokeWiki.Web.ApiClients
{
    public class ObjectsApiClient
    {
        private readonly HttpClient _httpClient;

        public ObjectsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<ObjetoDto>?> GetObjectsPageAsync(string? categoria, string? search, int page)
        {
            var url = $"api/objects?page={page}";
            if (!string.IsNullOrWhiteSpace(categoria)) url += $"&categoria={categoria}";
            if (!string.IsNullOrWhiteSpace(search)) url += $"&search={search}";

            return await _httpClient.GetFromJsonAsync<PagedResult<ObjetoDto>>(url);
        }

        public async Task<ObjetoDetalleDto?> GetObjectDetailAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ObjetoDetalleDto>($"api/objects/{id}");
        }
    }
}