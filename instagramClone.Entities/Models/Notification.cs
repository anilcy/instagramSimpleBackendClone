using instagramClone.Entities.Dtos;

namespace instagramClone.Entities.Models;

public class Notification
{
    public int Id { get; set; }
    public Guid RecipientId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = null!;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    
    // Related entities (optional, depending on notification type)
    public Guid? ActorId { get; set; }  // User who triggered the notification
    public int? PostId { get; set; }
    public int? CommentId { get; set; }
    
    public AppUser Recipient { get; set; } = null!;
    public AppUser? Actor { get; set; }
}