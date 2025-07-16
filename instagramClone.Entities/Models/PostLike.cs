namespace instagramClone.Entities.Models;

public class PostLike
{
    public Guid UserId { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    
    public AppUser User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}