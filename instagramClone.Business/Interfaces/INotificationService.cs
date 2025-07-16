using instagramClone.Entities.Dtos;

namespace instagramClone.Business.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<int> GetUnreadNotificationsCountAsync(Guid userId);
    Task MarkNotificationAsReadAsync(int notificationId, Guid userId);
    Task MarkAllNotificationsAsReadAsync(Guid userId);
    Task CreateLikeNotificationAsync(Guid actorId, Guid recipientId, int postId);
    Task CreateCommentNotificationAsync(Guid actorId, Guid recipientId, int postId, int commentId);
    Task CreateFollowNotificationAsync(Guid actorId, Guid recipientId);
    Task CreateCommentLikeNotificationAsync(Guid actorId, Guid recipientId, int commentId);
    Task CreateCommentReplyNotificationAsync(Guid actorId, Guid recipientId, int parentCommentId, int replyId);
}