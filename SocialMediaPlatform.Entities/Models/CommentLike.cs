using System;

namespace SocialMediaPlatform.Entities.Models;

public class CommentLike
{
    public CommentLike(Guid userId, Guid commentId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (commentId == Guid.Empty)
            throw new ArgumentException("CommentId cannot be empty.", nameof(commentId));
        UserId = userId;
        CommentId = commentId;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }
    private CommentLike() { }
    public Guid UserId { get; private set; }
    public Guid CommentId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    
    // status flag
    public bool IsDeleted { get; private set; } 
    
    // Navigation properties
    public AppUser User { get; private set; } = null!;
    public Comment Comment { get; private set; } = null!;
    
    public void SoftDelete()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
    }
}