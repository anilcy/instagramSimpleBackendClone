namespace instagramClone.Entities.Models;

public class CommentLike
{
    public Guid UserId { get; set; }
    public int CommentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    
    public AppUser User { get; set; } = null!;
    public Comment Comment { get; set; } = null!;
}