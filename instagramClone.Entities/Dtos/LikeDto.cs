namespace instagramClone.Entities.Dtos;

public class PostLikeDto
{
    public Guid UserId { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public UserSummaryDto User { get; set; } = null!;
}

public class CommentLikeDto
{
    public Guid UserId { get; set; }
    public int CommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public UserSummaryDto User { get; set; } = null!;
}

public class LikeActionDto
{
    public int EntityId { get; set; }  // PostId or CommentId
}