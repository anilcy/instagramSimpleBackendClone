namespace instagramClone.Entities.Dtos;

public class CommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Guid AuthorId { get; set; }
    public int? ParentCommentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    // Author information
    public UserSummaryDto Author { get; set; } = null!;
    
    // Statistics
    public int LikesCount { get; set; }
    public int RepliesCount { get; set; }
    
    // Current user's interaction
    public bool IsLikedByCurrentUser { get; set; }
    
    // Replies (can be included or excluded based on needs)
    public List<CommentDto>? Replies { get; set; }
}