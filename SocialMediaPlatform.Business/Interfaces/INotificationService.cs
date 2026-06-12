using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos.NotificationDtos;

namespace SocialMediaPlatform.Business.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<int> GetUnreadNotificationsCountAsync(Guid userId);
    Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllNotificationsAsReadAsync(Guid userId);
 
}