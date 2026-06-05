using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Dtos.FollowDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IFollowService
{
    Task<FollowDto> FollowUserAsync(Guid currentUserId, Guid targetUserId);
    Task<bool> UnfollowUserAsync(Guid currentUserId, Guid targetUserId);   
    Task<FollowResponseDto> RespondToFollowRequestAsync(Guid currentUserId, Guid requesterId, FollowStatus status);
    Task<List<FollowRequestDto>> GetFollowRequestsAsync(Guid userId, int page, int pageSize);
    Task<List<FollowDto>> GetFollowersAsync(Guid userId, int page, int pageSize);
    Task<List<FollowDto>> GetFollowingAsync(Guid userId, int page, int pageSize);
    Task<FollowStatus?> GetFollowStatusAsync(Guid currentUserId, Guid targetUserId);
    Task<int> GetFollowersCountAsync(Guid userId);
    Task<int> GetFollowingCountAsync(Guid userId);
    Task<bool> IsFollowingAsync(Guid currentUserId, Guid targetUserId);
}