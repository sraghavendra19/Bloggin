using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blogging.Client.Services
{
    public class CustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;

        public CustomAuthProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _js.InvokeAsync<string>("bloggingAuth.getToken");

            ClaimsIdentity identity;

            if (!string.IsNullOrWhiteSpace(token))
            {
                // Parse JWT or simply store email claim
                var email = ParseEmailFromToken(token); // create a simple parser function
                identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, email ?? ""),
                    new Claim("email", email ?? "")
                }, "jwt");
            }
            else
            {
                identity = new ClaimsIdentity(); // not authenticated
            }

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private string? ParseEmailFromToken(string token)
        {
            // Simple JWT parsing (without verification) to get email from payload
            try
            {
                var payload = token.Split('.')[1];
                var jsonBytes = System.Convert.FromBase64String(PadBase64(payload));
                var keyValues = System.Text.Json.JsonDocument.Parse(jsonBytes);
                if (keyValues.RootElement.TryGetProperty("email", out var emailProp))
                    return emailProp.GetString();
            }
            catch { }
            return null;
        }

        private static string PadBase64(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            base64 = base64.Replace('-', '+').Replace('_', '/');
            return base64;
        }
    }
}
