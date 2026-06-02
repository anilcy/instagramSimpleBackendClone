using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Entities.Dtos.MessageDtos;

public class ConversationDto
{
    public UserSummaryDto OtherUser { get; set; } = null!;
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}