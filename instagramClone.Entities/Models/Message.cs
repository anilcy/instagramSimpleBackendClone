namespace instagramClone.Entities.Models;

public class Message
{
    public int Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    
    public AppUser Receiver { get; set; } = null!;
    public AppUser Sender { get; set; } = null!;
}