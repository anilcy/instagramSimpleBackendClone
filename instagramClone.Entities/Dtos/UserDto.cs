namespace instagramClone.Entities.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginDate { get; set; }
    public bool IsActive { get; set; }
    
    // Statistics
    public int PostsCount { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    
    // Current user's relationship with this user
    public bool IsFollowing { get; set; }
    public FollowStatus? FollowStatus { get; set; }
}

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; }
}

public class UpdateUserProfileDto
{
    public string FullName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
}