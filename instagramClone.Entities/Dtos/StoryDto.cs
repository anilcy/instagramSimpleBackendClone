using System;

namespace instagramClone.Entities.Dtos.Story;

// Entities/Dtos/Story/StoryDto.cs
public class StoryDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string ProfilePictureUrl { get; set; } = null!;
    public string MediaUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

// Entities/Dtos/Story/StoryCreateDto.cs
public class StoryCreateDto
{
    public string MediaUrl { get; set; } = null!;
}

// Entities/Dtos/Story/StoryViewDto.cs  (opsiyonel)
public class StoryViewDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string ProfilePictureUrl { get; set; } = null!;
    public DateTime ViewedAt { get; set; }
}

