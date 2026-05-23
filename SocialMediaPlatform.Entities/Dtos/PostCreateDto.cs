using Microsoft.AspNetCore.Http;

namespace SocialMediaPlatform.Entities.Dtos;

public class PostCreateDto
{
    public required string Caption { get; set; }
    public required IFormFile ImageFile { get; set; }
}