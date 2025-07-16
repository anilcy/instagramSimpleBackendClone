namespace instagramClone.Entities.Models;

public class Notification
{
    public int Id { get; set; }
    public Guid RecipientId { get; set; }
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public int EntityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    
    public AppUser Recipient { get; set; } = null!;
    
}