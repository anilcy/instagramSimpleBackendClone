using AutoMapper;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos;

namespace instagramClone.Business.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository notificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize);
        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<int> GetUnreadNotificationsCountAsync(Guid userId)
    {
        return await _notificationRepository.GetUnreadNotificationsCountAsync(userId);
    }

    public async Task MarkNotificationAsReadAsync(int notificationId, Guid userId)
    {
        await _notificationRepository.MarkNotificationAsReadAsync(notificationId, userId);
    }

    public async Task MarkAllNotificationsAsReadAsync(Guid userId)
    {
        await _notificationRepository.MarkAllNotificationsAsReadAsync(userId);
    }

    public async Task CreateLikeNotificationAsync(Guid actorId, Guid recipientId, int postId)
    {
        if (actorId == recipientId) return; // Don't notify self

        await _notificationRepository.CreateNotificationAsync(
            recipientId, 
            NotificationType.Like, 
            "liked your post",
            $"/posts/{postId}",
            actorId,
            postId
        );
    }

    public async Task CreateCommentNotificationAsync(Guid actorId, Guid recipientId, int postId, int commentId)
    {
        if (actorId == recipientId) return; // Don't notify self

        await _notificationRepository.CreateNotificationAsync(
            recipientId,
            NotificationType.Comment,
            "commented on your post",
            $"/posts/{postId}#comment-{commentId}",
            actorId,
            postId,
            commentId
        );
    }

    public async Task CreateFollowNotificationAsync(Guid actorId, Guid recipientId)
    {
        await _notificationRepository.CreateNotificationAsync(
            recipientId,
            NotificationType.Follow,
            "started following you",
            $"/users/{actorId}",
            actorId
        );
    }

    public async Task CreateCommentLikeNotificationAsync(Guid actorId, Guid recipientId, int commentId)
    {
        if (actorId == recipientId) return; // Don't notify self

        await _notificationRepository.CreateNotificationAsync(
            recipientId,
            NotificationType.CommentLike,
            "liked your comment",
            null, // No specific URL for comment likes
            actorId,
            null,
            commentId
        );
    }

    public async Task CreateCommentReplyNotificationAsync(Guid actorId, Guid recipientId, int parentCommentId, int replyId)
    {
        if (actorId == recipientId) return; // Don't notify self

        await _notificationRepository.CreateNotificationAsync(
            recipientId,
            NotificationType.CommentReply,
            "replied to your comment",
            null, // No specific URL for comment replies
            actorId,
            null,
            replyId
        );
    }
}