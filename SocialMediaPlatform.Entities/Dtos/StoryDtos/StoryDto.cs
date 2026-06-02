using System;

namespace SocialMediaPlatform.Entities.Dtos.Story;

// Entities/Dtos/Story/StoryDto.cs
public class StoryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; }
    public string MediaUrl { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public int ViewsCount { get; set; }
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}





