using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;

public interface IMessageRepository : IGenericRepository<Message>
{
    // İki kullanıcı arası konuşma (sayfalı)
    Task<List<Message>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize);

    // Kullanıcının tüm karşı taraflarla son mesaj özetleri (listelemek için)
    Task<List<Message>> GetConversationsAsync(Guid userId);

    // Belirli bir karşı taraftan gelen okunmamış mesaj sayısı
    Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid fromUserId);

    // Belirli bir konuşmayı okunmuş işaretle
    Task MarkMessagesAsReadAsync(Guid userId, Guid fromUserId);

    // İki kullanıcı arasındaki son mesajı getir
    Task<Message?> GetLastMessageBetweenUsersAsync(Guid userId, Guid otherUserId);
}