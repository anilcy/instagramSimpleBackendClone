using Microsoft.AspNetCore.Http;

namespace instagramClone.Entities.Dtos;

public class PostCreateDto
{
    public string Caption { get; set; }
    public IFormFile ImageFile { get; set; }
}