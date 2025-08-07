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
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public FollowService(IFollowRepository followRepository, INotificationService notificationService,
        IUserRepository userRepository, IMapper mapper)
    {
        _followRepository = followRepository;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<FollowDto> FollowUserAsync(Guid currentUserId, Guid targetUserId)
    {
        if (currentUserId == targetUserId)
            throw new ArgumentException("Cannot follow yourself");

        var existingFollow = await _followRepository.GetFollowRelationshipAsync(currentUserId, targetUserId);
        if (existingFollow != null)
            throw new InvalidOperationException("Follow relationship already exists");

        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
            throw new ArgumentException("Target user not found");
        
        var follow = new Follow
        {
            FollowerId = currentUserId,
            FollowedId = targetUserId,
            Status = targetUser.IsPrivate ? FollowStatus.Pending : FollowStatus.Accepted,
            CreatedAt = DateTime.UtcNow
        };

        await _followRepository.InsertAsync(follow);
        await _followRepository.SaveChangesAsync();
        

        if (!targetUser.IsPrivate)
        {
            // Create notification immediately for public accounts
            await _notificationService.CreateFollowNotificationAsync(currentUserId, targetUserId);
        }

        return _mapper.Map<FollowDto>(follow);
    }

    public async Task<bool> UnfollowUserAsync(Guid currentUserId, Guid targetUserId)
    {
        var follow = await _followRepository.GetFollowRelationshipAsync(currentUserId, targetUserId);
        if (follow == null)
            return false;

        await _followRepository.DeleteAsync(follow);
        await _followRepository.SaveChangesAsync();
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
        await _followRepository.SaveChangesAsync();

        if (status == FollowStatus.Accepted)
        {
            await _notificationService.CreateFollowNotificationAsync(follow.FollowerId, follow.FollowedId);
        }

        return new FollowResponseDto
        {
            Success = true,
            Message = status == FollowStatus.Accepted ? "Follow request accepted" : "Follow request rejected",
            Status = status,
            Follow = _mapper.Map<FollowDto>(follow)
        };
    }

    public async Task<List<FollowRequestDto>> GetFollowRequestsAsync(Guid userId)
    {
        var followRequests = await _followRepository.GetPendingFollowRequestsAsync(userId);
        return _mapper.Map<List<FollowRequestDto>>(followRequests);
    }

    public async Task<List<FollowDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var followers = await _followRepository.GetFollowersAsync(userId, page, pageSize);
        return _mapper.Map<List<FollowDto>>(followers);
    }

    public async Task<List<FollowDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var following = await _followRepository.GetFollowingAsync(userId, page, pageSize);
        return _mapper.Map<List<FollowDto>>(following);
    }

    public async Task<int> GetFollowersCountAsync(Guid userId)
    {
        return await _followRepository.GetFollowersCountAsync(userId);
    }

    public async Task<int> GetFollowingCountAsync(Guid userId)
    {
        return await _followRepository.GetFollowingCountAsync(userId);
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