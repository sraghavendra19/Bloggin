using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blogging.Client.Services   // 👈 Add this
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public ApiService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        private async Task AddAuthHeader()
        {
            var token = await _js.InvokeAsync<string>("bloggingAuth.getToken");
            if (!string.IsNullOrEmpty(token))
            {
                if (_http.DefaultRequestHeaders.Contains("Authorization"))
                    _http.DefaultRequestHeaders.Remove("Authorization");
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Remove("Authorization");
            }
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            return await _http.GetFromJsonAsync<T>(url);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T payload, bool withAuth = false)
        {
            if (withAuth) await AddAuthHeader();
            return await _http.PostAsJsonAsync(url, payload);
        }
    }
}
