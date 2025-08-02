using instagramClone.Entities.Dtos.Story;

namespace instagramClone.Business.Interfaces;

public interface IStoryService
{
    Task<StoryDto> CreateStoryAsync(Guid userId, StoryCreateDto dto);
    Task<List<StoryDto>> GetUserActiveStoriesAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<List<StoryDto>> GetStoriesFeedAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<bool> AddStoryViewAsync(int storyId, Guid viewerId);
}