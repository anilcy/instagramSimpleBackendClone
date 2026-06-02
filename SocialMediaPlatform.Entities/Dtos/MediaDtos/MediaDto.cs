using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Entities.Dtos.MediaDtos;

public class MediaDto
{
    public Guid Id { get; set; }
    public string MediaUrl { get; set; } = null!;
    public MediaType Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}