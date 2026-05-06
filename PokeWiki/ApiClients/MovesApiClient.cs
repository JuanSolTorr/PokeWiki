using NugetPokeWiki.DTOs;
using System.Net.Http.Json;

namespace PokeWiki.Web.ApiClients
{
    public class MovesApiClient
    {
        private readonly HttpClient _httpClient;

        public MovesApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<MovimientosDto>?> GetMovesAsync(string? search)
        {
            var url = "api/moves";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"?search={search}";
            }
            return await _httpClient.GetFromJsonAsync<IEnumerable<MovimientosDto>>(url);
        }

        public async Task<MovimientosDto?> GetMoveAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<MovimientosDto>($"api/moves/{id}");
        }

        public async Task<IEnumerable<PokemonCompatibleMoveDto>?> GetCompatiblePokemonAsync(int moveId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<PokemonCompatibleMoveDto>>($"api/moves/{moveId}/compatible-pokemon");
        }
    }
}