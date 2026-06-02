using SocialMediaPlatform.Entities.Dtos.UserDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Entities.Dtos.FollowDtos;

public class FollowDto
{
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public FollowStatus Status { get; set; }
    public UserSummaryDto Follower { get; set; } = null!;
    public UserSummaryDto Followed { get; set; } = null!;
}

public class FollowRequestDto
{
    public Guid UserId { get; set; }
    public UserSummaryDto User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class FollowActionDto
{
    public Guid TargetUserId { get; set; }
}

public class FollowResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public FollowStatus Status { get; set; }
    public FollowDto? Follow { get; set; }
}