namespace instagramClone.Entities.Dtos;

public class FollowDto
{
    public Guid FollowerId { get; set; }
    public Guid FollowedId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
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
    public FollowRequestDto FollowRequest { get; set; } = null!;
    public FollowStatus Status { get; set; }
}