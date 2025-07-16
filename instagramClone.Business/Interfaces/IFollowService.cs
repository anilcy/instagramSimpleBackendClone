using instagramClone.Entities.Dtos;

namespace instagramClone.Business.Interfaces;

public interface IFollowService
{
    Task<FollowDto> FollowUserAsync(Guid currentUserId, Guid targetUserId);
    Task<bool> UnfollowUserAsync(Guid currentUserId, Guid targetUserId);
    Task<FollowResponseDto> RespondToFollowRequestAsync(Guid currentUserId, Guid requesterId, FollowStatus status);
    Task<List<FollowRequestDto>> GetPendingFollowRequestsAsync(Guid userId);
    Task<List<UserSummaryDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<List<UserSummaryDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<bool> IsFollowingAsync(Guid currentUserId, Guid targetUserId);
    Task<FollowStatus?> GetFollowStatusAsync(Guid currentUserId, Guid targetUserId);
}