using instagramClone.Entities.Models;

namespace instagramClone.Data.Interfaces;

public interface IStoryRepository : IGenericRepository<Story>
{
    Task<Story?> GetStoryAsync(int storyId);

    /// <summary>Yalnızca henüz süresi dolmamış (ExpiresAt > now) hikâyeler</summary>
    Task<List<Story>> GetUserActiveStoriesAsync(Guid userId,
        int page = 1,
        int pageSize = 20);

    /// <summary>Kullanıcının kendi + takip ettiklerinin aktif hikâyeleri</summary>
    Task<List<Story>> GetStoriesFeedAsync(Guid userId,
        int page = 1,
        int pageSize = 20);

    Task<int>  GetStoryViewCountAsync(int storyId);
    Task<bool> HasUserViewedStoryAsync(int storyId, Guid userId);

    /// <summary>Yeni görüntüleme kaydı ekler ( SaveChangesAsync base’te )</summary>
    Task AddStoryViewAsync(StoryView view);

    Task<List<StoryView>> GetStoryViewsAsync(int storyId,
        int page = 1,
        int pageSize = 50);
}