using AutoMapper;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public FollowService(IFollowRepository followRepository, INotificationService notificationService, IMapper mapper)
    {
        _followRepository = followRepository;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<FollowDto> FollowUserAsync(Guid currentUserId, Guid targetUserId)
    {
        if (currentUserId == targetUserId)
            throw new ArgumentException("Cannot follow yourself");

        var existingFollow = await _followRepository.GetFollowRelationshipAsync(currentUserId, targetUserId);
        if (existingFollow != null)
            throw new InvalidOperationException("Follow relationship already exists");

        var follow = new Follow
        {
            FollowerId = currentUserId,
            FollowedId = targetUserId,
            Status = FollowStatus.Accepted, // For now, auto-accept. Can be changed to Pending for private accounts
            CreatedAt = DateTime.UtcNow
        };

        var createdFollow = await _followRepository.AddAsync(follow);
        
        // Create notification
        await _notificationService.CreateFollowNotificationAsync(currentUserId, targetUserId);

        return _mapper.Map<FollowDto>(createdFollow);
    }

    public async Task<bool> UnfollowUserAsync(Guid currentUserId, Guid targetUserId)
    {
        var follow = await _followRepository.GetFollowRelationshipAsync(currentUserId, targetUserId);
        if (follow == null)
            return false;

        await _followRepository.DeleteAsync(follow);
        return true;
    }

    public async Task<FollowResponseDto> RespondToFollowRequestAsync(Guid currentUserId, Guid requesterId, FollowStatus status)
    {
        var follow = await _followRepository.GetFollowRelationshipAsync(requesterId, currentUserId);
        if (follow == null || follow.Status != FollowStatus.Pending)
            throw new ArgumentException("Follow request not found");

        follow.Status = status;
        follow.DecidedAt = DateTime.UtcNow;
        await _followRepository.UpdateAsync(follow);

        return new FollowResponseDto
        {
            FollowRequest = _mapper.Map<FollowRequestDto>(follow),
            Status = status
        };
    }

    public async Task<List<FollowRequestDto>> GetPendingFollowRequestsAsync(Guid userId)
    {
        var followRequests = await _followRepository.GetPendingFollowRequestsAsync(userId);
        return _mapper.Map<List<FollowRequestDto>>(followRequests);
    }

    public async Task<List<UserSummaryDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var followers = await _followRepository.GetFollowersAsync(userId, page, pageSize);
        return followers.Select(f => _mapper.Map<UserSummaryDto>(f.Follower)).ToList();
    }

    public async Task<List<UserSummaryDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var following = await _followRepository.GetFollowingAsync(userId, page, pageSize);
        return following.Select(f => _mapper.Map<UserSummaryDto>(f.Followed)).ToList();
    }

    public async Task<bool> IsFollowingAsync(Guid currentUserId, Guid targetUserId)
    {
        return await _followRepository.IsFollowingAsync(currentUserId, targetUserId);
    }

    public async Task<FollowStatus?> GetFollowStatusAsync(Guid currentUserId, Guid targetUserId)
    {
        var follow = await _followRepository.GetFollowRelationshipAsync(currentUserId, targetUserId);
        return follow?.Status;
    }
}