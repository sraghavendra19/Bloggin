using Blogging.Server.Services;
using Blogging.Shared.DTOs;
using Blogging.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace Blogging.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly MongoService _mongo;

        public PostController(MongoService mongo)
        {
            _mongo = mongo;
        }

        // ---------------- GET ALL POSTS ----------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _mongo.Posts
                .Find(_ => true)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(posts);
        }

        // ---------------- GET POST BY ID ----------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var post = await _mongo.Posts.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (post == null)
                return NotFound(new { message = "Post not found" });

            return Ok(post);
        }

        // ---------------- CREATE POST ----------------
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Post newPost)
        {
            var userId = User.FindFirstValue("id");
            var userEmail = User.FindFirstValue("email");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { message = "Invalid user context" });

            newPost.Id = Guid.NewGuid().ToString();
            newPost.AuthorId = userId;
            newPost.AuthorEmail = userEmail;
            newPost.CreatedAt = DateTime.UtcNow;

            await _mongo.Posts.InsertOneAsync(newPost);

            return Ok(newPost);
        }

        // ---------------- UPDATE POST ----------------
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Post updatedPost)
        {
            var userId = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid user context" });

            var existing = await _mongo.Posts.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existing == null)
                return NotFound(new { message = "Post not found" });

            if (existing.AuthorId != userId)
                return Forbid();

            var updateDef = Builders<Post>.Update
                .Set(p => p.Title, updatedPost.Title)
                .Set(p => p.Content, updatedPost.Content)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            await _mongo.Posts.UpdateOneAsync(p => p.Id == id, updateDef);

            return Ok(new { message = "Post updated" });
        }

        // ---------------- DELETE POST ----------------
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid user context" });

            var existing = await _mongo.Posts.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existing == null)
                return NotFound(new { message = "Post not found" });

            if (existing.AuthorId != userId)
                return Forbid();

            await _mongo.Posts.DeleteOneAsync(p => p.Id == id);

            return Ok(new { message = "Post deleted" });
        }

        // ---------------- ADD COMMENT ----------------
        [Authorize]
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(string id, [FromBody] AddCommentRequest req)
        {
            var userId = User.FindFirstValue("id");
            var userEmail = User.FindFirstValue("email");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { message = "Invalid user context" });

            var comment = new Comment
            {
                AuthorId = userId,
                AuthorEmail = userEmail,
                Text = req.Text,
                CreatedAt = DateTime.UtcNow
            };

            var updateDef = Builders<Post>.Update.Push(p => p.Comments, comment);
            var result = await _mongo.Posts.UpdateOneAsync(p => p.Id == id, updateDef);

            if (result.MatchedCount == 0)
                return NotFound(new { message = "Post not found" });

            return Ok(comment);
        }

        // ---------------- TOGGLE LIKE ----------------
        [Authorize]
        [HttpPost("{id}/like")]
        public async Task<IActionResult> ToggleLike(string id, [FromBody] ToggleLikeRequest req)
        {
            var userId = User.FindFirstValue("id");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid user context" });

            var post = await _mongo.Posts.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (post == null)
                return NotFound(new { message = "Post not found" });

            if (req.Like)
            {
                // Add like
                var update = Builders<Post>.Update.AddToSet(p => p.Likes, userId);
                await _mongo.Posts.UpdateOneAsync(p => p.Id == id, update);
            }
            else
            {
                // Remove like
                var update = Builders<Post>.Update.Pull(p => p.Likes, userId);
                await _mongo.Posts.UpdateOneAsync(p => p.Id == id, update);
            }

            var updatedPost = await _mongo.Posts.Find(p => p.Id == id).FirstOrDefaultAsync();
            return Ok(new { likesCount = updatedPost!.Likes.Count });
        }
    }
}
