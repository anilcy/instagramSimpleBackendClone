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
    private readonly IPrivacyService _privacyService;

    public StoryService(IStoryRepository storyRepo, IMapper mapper, IPrivacyService privacyService)
    {
        _storyRepo = storyRepo;
        _mapper = mapper;
        _privacyService = privacyService;
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

    public async Task<List<StoryDto>> GetUserActiveStoriesAsync(Guid targetUserId, Guid? requesterId, int page = 1, int pageSize = 20)
    {
        await _privacyService.EnsureCanAccessAsync(targetUserId, requesterId);

        var stories = await _storyRepo.GetUserActiveStoriesAsync(targetUserId, page, pageSize);
        return _mapper.Map<List<StoryDto>>(stories);
    }

    public async Task<List<StoryDto>> GetStoriesFeedAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var stories = await _storyRepo.GetStoriesFeedAsync(userId, page, pageSize);
        return _mapper.Map<List<StoryDto>>(stories);
    }

    public async Task<bool> AddStoryViewAsync(int storyId, Guid viewerId)
    {
        var story = await _storyRepo.GetStoryAsync(storyId);
        if (story == null)
            return false;

        await _privacyService.EnsureCanAccessAsync(story.UserId, viewerId);
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