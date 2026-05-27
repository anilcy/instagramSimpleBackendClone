using System;
using System.Collections.Generic;

namespace SocialMediaPlatform.Entities.Models;

public class Story
{
    public Story(Guid userId, string mediaUrl)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(mediaUrl))
            throw new ArgumentException("Media url cannot be empty.", nameof(mediaUrl));

        UserId = userId;
        MediaUrl = mediaUrl;
        CreatedAt = DateTimeOffset.UtcNow;
        ExpiresAt = DateTimeOffset.UtcNow.AddHours(24); // Default expiration is 24 hours from creation
        IsDeleted = false;
    }

    private Story() { }
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string MediaUrl { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    //Expiration check will be on repository
    public DateTimeOffset ExpiresAt { get; private set; } 
    public AppUser User { get; private set; } = null!;

    public ICollection<StoryView> Views { get; private set; } = new List<StoryView>();
    public ICollection<StoryLike> Likes { get; private set; } = new List<StoryLike>();
    
    public void SoftDeleteStory()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
    }
}
