using System.Net.Http.Json;
using Blogging.Shared.Models;

namespace Blogging.Client.Services
{
    public class PostService
    {
        private readonly HttpClient _http;

        public PostService(HttpClient http)
        {
            _http = http;
        }

        // Get all posts
        public async Task<List<Post>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<Post>>("api/post") ?? new();

        // Get post by id
        public async Task<Post?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<Post>($"api/post/{id}");

        // Create post
        public async Task<Post?> CreateAsync(Post newPost)
        {
            var res = await _http.PostAsJsonAsync("api/post", newPost);
            return await res.Content.ReadFromJsonAsync<Post>();
        }

        // Add comment
        public async Task AddCommentAsync(string postId, string text)
        {
            await _http.PostAsJsonAsync($"api/post/{postId}/comments", new { Text = text });
        }

        // Toggle like
        public async Task<int> ToggleLikeAsync(string postId, bool like)
        {
            var res = await _http.PostAsJsonAsync($"api/post/{postId}/like", new { Like = like });
            var obj = await res.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return obj?["likesCount"] ?? 0;
        }
    }
}
