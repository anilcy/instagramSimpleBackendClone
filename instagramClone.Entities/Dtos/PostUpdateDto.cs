using Microsoft.AspNetCore.Http;

namespace instagramClone.Entities.Dtos;
public class PostUpdateDto
{
    public string? Caption { get; set; }
    public IFormFile? ImageFile { get; set; } // Yeni resim gönderilebilir, null ise eski resim korunur.
}
