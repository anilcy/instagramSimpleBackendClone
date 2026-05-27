using System;
using System.Collections.Generic;

namespace SocialMediaPlatform.Entities.Models;

public class Post
{
    public Post(Guid authorId, string mediaUrl, string? caption)
    {
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author id cannot be empty.", nameof(authorId));
        if (string.IsNullOrWhiteSpace(mediaUrl))
            throw new ArgumentException("Media url cannot be empty.", nameof(mediaUrl));
        
        AuthorId = authorId;
        MediaUrl = mediaUrl;
        Caption = caption;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }
    private Post () { }
    public int Id { get; private set; }
    public Guid AuthorId { get; private set; }
    public string MediaUrl { get; private set; } 
    public string? Caption { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; } 
    public bool IsDeleted { get; private set; } 
    public DateTimeOffset? DeletedAt { get; private set; }
    
    public AppUser Author { get; private set; } = null!;
    
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; private set; } = new List<PostLike>();
    
    public void UpdatePost(string? newCaption, string? newMediaUrl)
    {
        if (string.IsNullOrWhiteSpace(newMediaUrl))
            throw new ArgumentException("Media url cannot be empty.", nameof(newMediaUrl));
        
        Caption = newCaption;
        MediaUrl = newMediaUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDeletePost()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
    }
}
