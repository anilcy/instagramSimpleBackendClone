using instagramClone.Entities.Dtos;

namespace instagramClone.Business.Interfaces;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(Guid senderId, CreateMessageDto dto);
    Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50);
    Task<MessageDto?> GetByIdAsync(int id);
    Task<bool> MarkAsReadAsync(int messageId, Guid readerId);
    Task<int> GetUnreadCountAsync(Guid userId, Guid fromUserId);
    Task MarkMessagesAsReadAsync(Guid userId, Guid fromUserId);
}