namespace instagramClone.Entities.Dtos;

public class PostDto
{
    public int Id { get; set; }
    public Guid AuthorId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    
    // Author information
    public UserSummaryDto Author { get; set; } = null!;
    
    // Statistics
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    
    // Current user's interaction
    public bool IsLikedByCurrentUser { get; set; }
    
    // Comments (can be included or excluded based on needs)
    public List<CommentDto>? Comments { get; set; }
}

public class PostSummaryDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserSummaryDto Author { get; set; } = null!;
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}