using instagramClone.Entities.Dtos;

namespace instagramClone.Business.Interfaces;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(Guid senderId, CreateMessageDto messageDto);
    Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50);
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
    Task MarkMessagesAsReadAsync(Guid userId, Guid fromUserId);
    Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid fromUserId);
}