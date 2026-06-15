using System;
using System.Threading.Tasks;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IPresenceService
{
    Task SetOnlineAsync(Guid userId, string connectionId);
    Task SetOfflineAsync(Guid userId, string connectionId);
    Task<bool> IsOnlineAsync(Guid userId);
    Task<DateTimeOffset?> GetLastSeenAsync(Guid userId);
}