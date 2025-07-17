using Microsoft.AspNetCore.Http;

namespace instagramClone.Entities.Dtos;

public class PostCreateDto
{
    public required string Caption { get; set; }
    public required IFormFile ImageFile { get; set; }
}