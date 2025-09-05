using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Blogging.Shared.DTOs;
using Blogging.Shared.Models;
using Blogging.Server.Services;

namespace Blogging.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly MongoService _mongo;

        public PostsController(MongoService mongo)
        {
            _mongo = mongo;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicPosts()
        {
            var posts = await _mongo.Posts
                .Find(p => p.Privacy == "public")
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(posts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostRequest req)
        {
            var userId = User.FindFirst("id")?.Value;
            var email = User.FindFirst("email")?.Value;

            if (userId == null) return Unauthorized();

            var post = new Post
            {
                Id = Guid.NewGuid().ToString(),
                AuthorId = userId,
                AuthorEmail = email ?? "",
                Title = req.Title,
                Markdown = req.Markdown,
                CreatedAt = DateTime.UtcNow,
                Privacy = string.IsNullOrWhiteSpace(req.Privacy) ? "public" : req.Privacy
            };

            await _mongo.Posts.InsertOneAsync(post);
            return Ok(post);
        }
    }
}
