using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Entities.Dtos.CommentDtos;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public UserSummaryDto Author { get; set; } = null!;
    
    public int LikesCount { get; set; }
    public int RepliesCount { get; set; }
    
    public bool IsLikedByCurrentUser { get; set; }
    
    public List<CommentDto>? Replies { get; set; }
}

