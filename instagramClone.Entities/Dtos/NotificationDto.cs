namespace instagramClone.Entities.Dtos;

public class NotificationDto
{
    public int Id { get; set; }
    public Guid RecipientId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = null!;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Related entities (optional, depending on notification type)
    public UserSummaryDto? Actor { get; set; }  // User who triggered the notification
    public int? PostId { get; set; }
    public int? CommentId { get; set; }
}

public enum NotificationType
{
    Like,           // Someone liked your post
    Comment,        // Someone commented on your post
    Follow,         // Someone followed you
    FollowRequest,  // Someone requested to follow you
    CommentLike,    // Someone liked your comment
    CommentReply    // Someone replied to your comment
}