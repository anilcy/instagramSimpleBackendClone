using SocialMediaPlatform.Entities.Dtos.UserDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Entities.Dtos.NotificationDtos;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    // Related entities (optional, depending on notification type)
    public UserSummaryDto? Actor { get; set; }  // User who triggered the notification
    public int? PostId { get; set; }
    public int? CommentId { get; set; }
}

