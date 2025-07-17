using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Interfaces;

public interface IFollowService
{
    Task<FollowDto> FollowUserAsync(Guid currentUserId, Guid targetUserId);
    Task<FollowResponseDto> RespondToFollowRequestAsync(Guid currentUserId, Guid requesterId, FollowStatus status);
    Task<bool> UnfollowUserAsync(Guid currentUserId, Guid targetUserId);
    
    Task<List<FollowDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<List<FollowDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<FollowStatus?> GetFollowStatusAsync(Guid currentUserId, Guid targetUserId);
    
    Task<List<FollowRequestDto>> GetFollowRequestsAsync(Guid userId);
    Task<int> GetFollowersCountAsync(Guid userId);
    Task<int> GetFollowingCountAsync(Guid userId);
    Task<bool> IsFollowingAsync(Guid currentUserId, Guid targetUserId);
}