using Blogging.Shared.DTOs;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Blogging.Client.Services   // 👈 Add this namespace wrapper
{
    public class AuthService
    {
        private readonly ApiService _api;
        private readonly IJSRuntime _js;

        public AuthService(ApiService api, IJSRuntime js)
        {
            _api = api;
            _js = js;
        }

        public async Task<string?> Register(string email, string password)
        {
            var resp = await _api.PostAsync("/api/auth/register", new RegisterRequest(email, password));
            if (resp.IsSuccessStatusCode) return null;
            var text = await resp.Content.ReadAsStringAsync();
            return text;
        }

        public async Task<bool> Login(string email, string password)
        {
            var resp = await _api.PostAsync("/api/auth/login", new LoginRequest(email, password));
            if (!resp.IsSuccessStatusCode) return false;

            var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth != null)
            {
                await _js.InvokeVoidAsync("bloggingAuth.setToken", auth.Token);
                return true;
            }
            return false;
        }

        public async Task Logout()
        {
            await _js.InvokeVoidAsync("bloggingAuth.removeToken");
        }

        public async Task<string?> GetToken()
        {
            return await _js.InvokeAsync<string>("bloggingAuth.getToken");
        }
    }
}
