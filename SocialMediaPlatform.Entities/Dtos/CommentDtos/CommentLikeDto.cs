using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Entities.Dtos.CommentDtos;

public class CommentLikeDto
{
    public Guid UserId { get; set; }
    public Guid CommentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public UserSummaryDto User { get; set; } = null!;
}