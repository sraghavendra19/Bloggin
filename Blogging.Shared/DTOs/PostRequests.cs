namespace Blogging.Shared.DTOs
{
    public record CreatePostRequest(string Title, string Markdown, string Privacy);
}
