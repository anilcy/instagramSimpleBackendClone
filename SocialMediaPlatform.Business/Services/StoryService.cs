using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.Story;
using SocialMediaPlatform.Entities.Dtos.StoryDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Business.Services;

public class StoryService : IStoryService
{
    private readonly IStoryRepository _storyRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;
    private readonly IPrivacyService _privacyService;
    private readonly IUnitOfWork _unitOfWork;

    public StoryService(
        IStoryRepository storyRepository,
        INotificationRepository notificationRepository,
        IFileStorageService fileStorageService,
        IMapper mapper,
        IPrivacyService privacyService,
        IUnitOfWork unitOfWork)
    {
        _storyRepository = storyRepository;
        _notificationRepository = notificationRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
        _privacyService = privacyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<StoryDto> CreateStoryAsync(Guid userId, IFormFile mediaFile)
    {
        var mediaUrl = await _fileStorageService.UploadFileAsync(mediaFile);
        var story = new Story(userId, mediaUrl);

        _storyRepository.Add(story);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<StoryDto>(story);
    }

    public async Task<List<StoryDto>> GetUserActiveStoriesAsync(Guid targetUserId, Guid? requesterId, int page = 1, int pageSize = 20)
    {
        await _privacyService.EnsureCanAccessAsync(targetUserId, requesterId);

        var stories = await _storyRepository.GetUserActiveStoriesAsync(targetUserId, page, pageSize);
        return _mapper.Map<List<StoryDto>>(stories);
    }

    public async Task<List<StoryDto>> GetStoriesFeedAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var stories = await _storyRepository.GetStoriesFeedAsync(userId, page, pageSize);
        return _mapper.Map<List<StoryDto>>(stories);
    }

    public async Task AddStoryViewAsync(Guid storyId, Guid viewerId)
    {
        var story = await _storyRepository.GetStoryAsync(storyId);
        if (story == null)
            throw new KeyNotFoundException("Story not found.");

        await _privacyService.EnsureCanAccessAsync(story.UserId, viewerId);
        
        if (await _storyRepository.HasUserViewedStoryAsync(storyId, viewerId))
            return;

        var view = new StoryView(storyId, viewerId);

        _storyRepository.AddStoryView(view);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task DeleteStoryAsync(Guid storyId, Guid userId)
    {
        var story = await _storyRepository.GetStoryAsync(storyId);
        if (story == null)
            throw new KeyNotFoundException("Story not found.");
        if (story.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own stories.");

        story.SoftDeleteStory();
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task LikeStoryAsync(Guid userId, Guid storyId)
    {
        var story = await _storyRepository.GetStoryAsync(storyId);
        if (story == null)
            throw new KeyNotFoundException("Story not found.");

        await _privacyService.EnsureCanAccessAsync(story.UserId, userId);

        var existingLike = await _storyRepository.GetStoryLikeAsync(storyId, userId);
        if (existingLike != null)
            throw new InvalidOperationException("Already liked.");

        var like = new StoryLike(userId, storyId);
        _storyRepository.AddStoryLike(like);

        if (story.UserId != userId)
        {
            var notification = Notification.StoryLikeNotification(story.UserId, userId, storyId);
            _notificationRepository.Add(notification);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnlikeStoryAsync(Guid userId, Guid storyId)
    {
        var like = await _storyRepository.GetStoryLikeAsync(storyId, userId);
        if (like == null)
            throw new KeyNotFoundException("Like not found.");

        like.SoftDeleteStoryLike();
        await _unitOfWork.SaveChangesAsync();
    }
}