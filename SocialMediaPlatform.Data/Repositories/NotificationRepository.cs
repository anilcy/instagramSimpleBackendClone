using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaPlatform.Data.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(SocialMediaDbContext context) : base(context)
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
    
}