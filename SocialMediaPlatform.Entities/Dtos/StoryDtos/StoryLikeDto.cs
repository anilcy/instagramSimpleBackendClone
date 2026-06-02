using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Entities.Dtos;

public class StoryLikeDto
{
    public Guid UserId { get; set; }
    public Guid StoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public UserSummaryDto User { get; set; } = null!;
}