using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize);
    Task<int> GetUnreadNotificationsCountAsync(Guid userId);

}