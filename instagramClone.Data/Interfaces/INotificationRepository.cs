using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;

namespace instagramClone.Data.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize);
    Task<int> GetUnreadNotificationsCountAsync(Guid userId);
    Task MarkNotificationAsReadAsync(int notificationId, Guid userId);
    Task MarkAllNotificationsAsReadAsync(Guid userId);
    Task CreateNotificationAsync(Guid recipientId, NotificationType type, string message, 
        string? actionUrl = null, Guid? actorId = null, int? postId = null, int? commentId = null);
}