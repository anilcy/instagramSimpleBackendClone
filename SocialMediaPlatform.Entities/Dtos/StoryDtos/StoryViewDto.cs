namespace SocialMediaPlatform.Entities.Dtos.UserDtos;

public class StoryViewDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; } 
    public DateTimeOffset ViewedAt { get; set; }
}