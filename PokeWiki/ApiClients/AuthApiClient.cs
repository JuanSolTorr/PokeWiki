using NugetPokeWiki.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PokeWiki.Web.ApiClients
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }
            return null;
        }

        public async Task<AuthResponseDto?> RegisterAsync(UserRegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }
            return null;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAccountAsync(string password, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage(HttpMethod.Delete, "api/auth/delete-account")
            {
                Content = JsonContent.Create(password)
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}