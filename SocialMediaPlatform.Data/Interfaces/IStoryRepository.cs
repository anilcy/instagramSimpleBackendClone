using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;

public interface IStoryRepository : IGenericRepository<Story>
{
    Task<Story?> GetStoryAsync(Guid storyId);

    /// <summary>Yalnızca henüz süresi dolmamış (ExpiresAt > now) hikâyeler</summary>
    Task<List<Story>> GetUserActiveStoriesAsync(Guid userId,
        int page = 1,
        int pageSize = 20);

    /// <summary>Kullanıcının kendi + takip ettiklerinin aktif hikâyeleri</summary>
    Task<List<Story>> GetStoriesFeedAsync(Guid userId,
        int page = 1,
        int pageSize = 20);

    Task<int>  GetStoryViewCountAsync(Guid storyId);
    Task<bool> HasUserViewedStoryAsync(Guid storyId, Guid userId);

    /// <summary>Yeni görüntüleme kaydı ekler ( SaveChangesAsync base’te )</summary>
    Task AddStoryViewAsync(StoryView view);

    Task<List<StoryView>> GetStoryViewsAsync(Guid storyId,
        int page = 1,
        int pageSize = 50);
}