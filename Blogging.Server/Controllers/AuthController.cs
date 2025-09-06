using Blogging.Server.Services;
using Blogging.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using MongoDB.Driver;
using Blogging.Shared.DTOs;

namespace Blogging.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MongoService _mongo;
        private readonly IConfiguration _config;

        public AuthController(MongoService mongo, IConfiguration config)
        {
            _mongo = mongo;
            _config = config;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var existing = await _mongo.Users.Find(u => u.Email == req.Email).FirstOrDefaultAsync();
            if (existing != null) return BadRequest(new { message = "Email already registered" });

            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _mongo.Users.InsertOneAsync(newUser);
            return Ok(new { message = "Registered successfully" });
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _mongo.Users.Find(u => u.Email == req.Email).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    email = user.Email
                }
            });
        }

        // ---------------- TOKEN GENERATOR ----------------
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? "this_is_a_super_long_default_key_1234567890!";
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            var claims = new[]
            {
                new Claim("id", user.Id),
                new Claim("email", user.Email)
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
