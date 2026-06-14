using System;

namespace SocialMediaPlatform.Entities.Models;

public class StoryView
{
    public StoryView(Guid storyId, Guid userId)
    {
        if (storyId == Guid.Empty)
            throw new ArgumentException("Story id cannot be empty.", nameof(storyId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        StoryId = storyId;
        UserId = userId;
        ViewedAt = DateTimeOffset.UtcNow;
    }
    private StoryView() { }

    public Guid StoryId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset ViewedAt { get; private set; }     
    public Story Story { get; private set; } = null!;
    public AppUser User { get; private set; } = null!;
}