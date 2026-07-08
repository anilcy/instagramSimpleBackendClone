using System;
using System.Collections.Generic;

namespace SocialMediaPlatform.Entities.Models;

public class Comment
{
    public Comment(Guid postId, Guid authorId, string content, Guid? parentCommentId = null)
    {
        // Guid id will be created automatically by database
        if (postId == Guid.Empty)
            throw new ArgumentException("Post Id cannot be empty.", nameof(postId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("Author Id cannot be empty.", nameof(authorId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));
        
        PostId = postId;
        AuthorId = authorId;
        Content = content;
        ParentCommentId = parentCommentId;
        LikeCount = 0; //??
        CreatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }

    private Comment() { }
    public Guid Id { get; private set; }
    public Guid PostId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public int LikeCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } 
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    //Status flag
    public bool IsDeleted { get; private set; } 
    
    //Navigation Properties
    public AppUser Author { get; private set; } = null!;
    public Post Post { get; private set; } = null!;
    public Comment? Parent { get; private set; }
        
    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();
    public ICollection<CommentLike> Likes { get; private set; } = new List<CommentLike>();
    
    public void UpdateContent(string newContent)
    {
        if(string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Content cannot be empty.", nameof(newContent));
        Content = newContent;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void SoftDeleteComment()
    {
        if (IsDeleted)
            return;
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}


/*namespace SocialMediaPlatform.Entities.Models;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }

    public Guid AuthorId { get; set; }

    public int? ParentCommentId { get; set; }
    public Comment? Parent { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    
    
    public AppUser Author { get; set; } = null!;
    public Post Post { get; set; } = null!;
        
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}*/

