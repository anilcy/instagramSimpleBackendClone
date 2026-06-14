using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Entities.Dtos.UserDtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; } 
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastLoginDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrivate { get; set; }
    
    // Statistics
    public int PostsCount { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    
    // Current user's relationship with this user
    public FollowStatus? FollowStatus { get; set; }
}

