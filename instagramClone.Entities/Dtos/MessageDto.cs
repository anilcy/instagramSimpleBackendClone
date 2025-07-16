namespace instagramClone.Entities.Dtos;

public class MessageDto
{
    public int Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    
    public UserSummaryDto Sender { get; set; } = null!;
    public UserSummaryDto Receiver { get; set; } = null!;
}

public class CreateMessageDto
{
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = null!;
}

public class ConversationDto
{
    public UserSummaryDto OtherUser { get; set; } = null!;
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}