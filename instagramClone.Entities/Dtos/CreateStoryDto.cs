using Microsoft.AspNetCore.Http;

namespace instagramClone.Entities.Dtos.Story;

public class CreateStoryDto
{
    public IFormFile MediaFile { get; set; }
}
