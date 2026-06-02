using SocialMediaPlatform.Entities.Dtos.CommentDtos;
using SocialMediaPlatform.Entities.Dtos.MediaDtos;
using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Entities.Dtos.PostDtos;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string? Caption { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public UserSummaryDto Author { get; set; } = null!;
    public List<MediaDto>? MediaItems { get; set; }
    // Statistics
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    
    // Comments (can be included or excluded based on needs)
    public List<CommentDto>? Comments { get; set; }
}
