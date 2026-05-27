using System;

namespace SocialMediaPlatform.Entities.Models;

public class PostLike
{
    public PostLike(Guid userId, Guid postId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User Id cannot be empty.", nameof(userId));

        if (postId == Guid.Empty)
            throw new ArgumentException("Post Id cannot be empty.", nameof(postId));

        UserId = userId;
        PostId = postId;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }

    private PostLike() { }

    public Guid UserId { get; private set; }
    public Guid PostId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; } 
    
    //Navigation properties
    public AppUser User { get; private set; } = null!;
    public Post Post { get; private set; } = null!;
    
    public void SoftDeletePostLike()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
    }
}