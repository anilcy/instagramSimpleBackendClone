using System;

namespace SocialMediaPlatform.Entities.Models;

public class StoryLike
{
    public StoryLike(Guid userId, Guid storyId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("userId cannot be empty.",nameof(userId));
        if (storyId == Guid.Empty)
            throw new ArgumentException("storyId cannot be empty.",nameof(storyId));
        UserId = userId;
        StoryId = storyId;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }
    private StoryLike() { }
    public Guid UserId { get; private set; }
    public Guid StoryId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    public Story Story { get; private set; } = null!;
    public AppUser User { get; private set; } = null!;
    
    public void SoftDeleteStoryLike()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
    }
}