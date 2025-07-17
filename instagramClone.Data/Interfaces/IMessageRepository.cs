using instagramClone.Entities.Models;

namespace instagramClone.Data.Interfaces;

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<List<Message>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize);
    Task<List<Message>> GetConversationsAsync(Guid userId);
    Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid fromUserId);
    Task MarkMessagesAsReadAsync(Guid userId, Guid fromUserId);
    Task<Message?> GetLastMessageBetweenUsersAsync(Guid userId, Guid otherUserId);
}