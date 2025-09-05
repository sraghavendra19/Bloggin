using System;

namespace Blogging.Shared.Models
{
    public class Post
    {
        public string Id { get; set; } = null!;
        public string AuthorId { get; set; } = null!;
        public string AuthorEmail { get; set; } = null!; // convenience
        public string Title { get; set; } = null!;
        public string Markdown { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Privacy { get; set; } = "public"; // public | followed | private
    }
}
