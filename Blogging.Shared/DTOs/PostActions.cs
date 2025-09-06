namespace Blogging.Shared.DTOs
{
    public record AddCommentRequest(string Text);
    public record ToggleLikeRequest(bool Like); // true = like, false = unlike
}
