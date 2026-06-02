namespace SocialMediaPlatform.Entities.Dtos.UserDtos;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string? FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}