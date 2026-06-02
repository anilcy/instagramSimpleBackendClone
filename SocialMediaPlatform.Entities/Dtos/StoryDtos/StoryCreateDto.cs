using Microsoft.AspNetCore.Http;

namespace SocialMediaPlatform.Entities.Dtos.StoryDtos;

public class StoryCreateDto
{
    public required IFormFile MediaFile { get; set; }
}
