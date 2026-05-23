using Microsoft.AspNetCore.Http;

namespace SocialMediaPlatform.Entities.Dtos.Story;

public class CreateStoryDto
{
    public IFormFile MediaFile { get; set; }
}
