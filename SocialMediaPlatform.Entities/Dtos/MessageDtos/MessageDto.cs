using SocialMediaPlatform.Entities.Dtos.MediaDtos;
using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Entities.Dtos.MessageDtos;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string? Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }    
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    
    public UserSummaryDto Sender { get; set; } = null!;
    public UserSummaryDto Receiver { get; set; } = null!;
    public List<MediaDto>? MediaItems { get; set; }
}
