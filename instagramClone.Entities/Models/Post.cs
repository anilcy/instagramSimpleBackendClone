namespace instagramClone.Entities.Models;

public class Post
{
    public int Id { get; set; }
    
    public Guid AuthorId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    
    public AppUser Author { get; set; } = null!;
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
}

