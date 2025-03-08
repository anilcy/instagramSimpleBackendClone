namespace instagramClone.Entities.Models;

public class Post
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public string Caption { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;


}

