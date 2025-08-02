using AutoMapper;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos.Story;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Services;

public class StoryService : IStoryService
{
    private readonly IStoryRepository _storyRepo;
    private readonly IMapper _mapper;

    public StoryService(IStoryRepository storyRepo, IMapper mapper)
    {
        _storyRepo = storyRepo;
        _mapper = mapper;
    }

    public async Task<StoryDto> CreateStoryAsync(Guid userId, StoryCreateDto dto)
    {
        var story = _mapper.Map<Story>(dto);
        story.UserId = userId;
        story.CreatedAt = DateTime.UtcNow;
        story.ExpiresAt = story.CreatedAt.AddHours(24);

        await _storyRepo.InsertAsync(story);
        await _storyRepo.SaveChangesAsync();

        return _mapper.Map<StoryDto>(story);
    }

    public async Task<List<StoryDto>> GetUserActiveStoriesAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var stories = await _storyRepo.GetUserActiveStoriesAsync(userId, page, pageSize);
        return _mapper.Map<List<StoryDto>>(stories);
    }

    public async Task<List<StoryDto>> GetStoriesFeedAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var stories = await _storyRepo.GetStoriesFeedAsync(userId, page, pageSize);
        return _mapper.Map<List<StoryDto>>(stories);
    }

    public async Task<bool> AddStoryViewAsync(int storyId, Guid viewerId)
    {
        // Zaten izlediyse tekrar ekleme
        if (await _storyRepo.HasUserViewedStoryAsync(storyId, viewerId))
            return false;

        var view = new StoryView
        {
            StoryId = storyId,
            UserId  = viewerId,
            ViewedAt = DateTime.UtcNow
        };

        await _storyRepo.AddStoryViewAsync(view);
        await _storyRepo.SaveChangesAsync();
        return true;
    }
}