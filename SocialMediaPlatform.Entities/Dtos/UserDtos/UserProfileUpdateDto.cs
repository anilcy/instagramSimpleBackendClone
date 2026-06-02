using Microsoft.AspNetCore.Http;

namespace SocialMediaPlatform.Entities.Dtos.UserDtos;

public class UserProfileUpdateDto
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}