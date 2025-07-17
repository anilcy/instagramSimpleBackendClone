using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(InstagramDbContext context) : base(context)
    {
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize)
    {
        return await _context.Notifications
            .Where(n => n.RecipientId == userId)
            .Include(n => n.Actor)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUnreadNotificationsCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.RecipientId == userId && !n.IsRead);
    }

    public async Task MarkNotificationAsReadAsync(int notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);
        
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllNotificationsAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(Guid recipientId, NotificationType type, string message, 
        string? actionUrl = null, Guid? actorId = null, int? postId = null, int? commentId = null)
    {
        var notification = new Notification
        {
            RecipientId = recipientId,
            Type = type,
            Message = message,
            ActionUrl = actionUrl,
            ActorId = actorId,
            PostId = postId,
            CommentId = commentId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }
}