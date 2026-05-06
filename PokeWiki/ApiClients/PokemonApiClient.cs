using NugetPokeWiki.DTOs;
using System.Net.Http.Json;

namespace PokeWiki.Web.ApiClients
{
    public class PokemonApiClient
    {
        private readonly HttpClient _httpClient;

        public PokemonApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<PokemonListDto>?> GetPokemonPageAsync(string? search, int page)
        {
            var url = $"api/pokemon?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={search}";
            }

            return await _httpClient.GetFromJsonAsync<PagedResult<PokemonListDto>>(url);
        }

        public async Task<PokemonDetailDto?> GetPokemonDetailsAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PokemonDetailDto>($"api/pokemon/{id}");
        }
    }
}