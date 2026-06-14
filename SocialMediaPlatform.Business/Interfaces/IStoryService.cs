using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SocialMediaPlatform.Entities.Dtos.Story;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IStoryService
{
    Task<StoryDto> CreateStoryAsync(Guid userId, IFormFile mediaFile);
    Task<List<StoryDto>> GetUserActiveStoriesAsync(Guid targetUserId, Guid? requesterId, int page = 1, int pageSize = 20);
    Task<List<StoryDto>> GetStoriesFeedAsync(Guid userId, int page = 1, int pageSize = 20);
    Task AddStoryViewAsync(Guid storyId, Guid viewerId);
    Task DeleteStoryAsync(Guid storyId, Guid userId);
    Task LikeStoryAsync(Guid userId, Guid storyId);
    Task UnlikeStoryAsync(Guid userId, Guid storyId);

}