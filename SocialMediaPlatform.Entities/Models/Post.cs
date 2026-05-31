using System;
using System.Collections.Generic;

namespace SocialMediaPlatform.Entities.Models;

public class Post
{
    public Post(Guid authorId, string? caption)
    {
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author id cannot be empty.", nameof(authorId));

        AuthorId = authorId;
        Caption = caption;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }
    private Post () { }
    public Guid Id { get; private set; }
    public Guid AuthorId { get; private set; }
    public string? Caption { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; } 
    public bool IsDeleted { get; private set; } 
    public DateTimeOffset? DeletedAt { get; private set; }
    
    public AppUser Author { get; private set; } = null!;
    public ICollection<Media> MediaItems { get; private set; } = new List<Media>();
    
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; private set; } = new List<PostLike>();
    
    //public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
    // we wont use post.Netifications so no need for this. the relationship is already defined from Notifications entity
    
    public void UpdatePost(string? newCaption)
    {
        Caption = newCaption;
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
