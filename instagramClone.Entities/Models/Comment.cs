namespace instagramClone.Entities.Models;

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
}
