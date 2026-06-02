using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Entities.Dtos.PostDtos;

public class PostLikeDto
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public UserSummaryDto User { get; set; } = null!;
}