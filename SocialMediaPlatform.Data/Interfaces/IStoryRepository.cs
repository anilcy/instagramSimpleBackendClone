using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;

public interface IStoryRepository : IGenericRepository<Story>
{
    Task<Story?> GetStoryAsync(Guid storyId);
    Task<List<Story>> GetUserActiveStoriesAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<List<Story>> GetStoriesFeedAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<int>  GetStoryViewCountAsync(Guid storyId);
    Task<bool> HasUserViewedStoryAsync(Guid storyId, Guid userId);
    void AddStoryView(StoryView view);
    void AddStoryLike(StoryLike like);
    Task<List<StoryView>> GetStoryViewsAsync(Guid storyId, int page = 1, int pageSize = 50);
    Task<StoryLike?> GetStoryLikeAsync(Guid storyId, Guid userId);
}