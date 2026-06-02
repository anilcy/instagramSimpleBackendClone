using Microsoft.AspNetCore.Http;

namespace SocialMediaPlatform.Entities.Dtos.PostDtos;

public class PostCreateDto
{
    public string? Caption { get; set; }
    public required List<IFormFile> MediaFiles { get; set; }
}