namespace SocialMediaPlatform.Entities.Dtos;

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

