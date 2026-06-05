using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.FollowDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Business.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly SocialMediaDbContext _dbContext;

    public FollowService(
        IFollowRepository followRepository, 
        INotificationRepository notificationRepository,
        IUserRepository userRepository, 
        IMapper mapper, 
        SocialMediaDbContext dbContext)
    {
        _followRepository = followRepository;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _dbContext = dbContext;
        
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


        Follow follow;
        if (!targetUser.IsPrivate)
        {
            follow = new Follow(currentUserId, targetUserId, false);
            _followRepository.Add(follow);
            // Create notification immediately for public accounts
            var notification = Notification.FollowNotification(targetUserId, currentUserId);
            _notificationRepository.Add(notification);
        }
        else
        {
            follow = new Follow(currentUserId, targetUserId, true);
            _followRepository.Add(follow);
            // Create follow request notification for private accounts
            var notification = Notification.FollowRequestNotification(targetUserId, currentUserId);
            _notificationRepository.Add(notification);
        }
        
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<FollowDto>(follow);
    }

    public async Task<bool> UnfollowUserAsync(Guid currentUserId, Guid targetUserId)
    {
        var follow = await _followRepository.GetFollowRelationshipAsync(currentUserId, targetUserId);
        if (follow == null)
            return false;

        follow.SoftDeleteFollow();
        await _dbContext.SaveChangesAsync();
        return true; 
    }

    public async Task<FollowResponseDto> RespondToFollowRequestAsync(Guid currentUserId, Guid requesterId, FollowStatus status)
    {
        var follow = await _followRepository.GetFollowRelationshipAsync(requesterId, currentUserId);
        if (follow == null || follow.Status != FollowStatus.Pending)
            throw new ArgumentException("Follow request not found");

        if (status == FollowStatus.Accepted)
        {
            follow.AcceptRequest();
            var notification = Notification.FollowAcceptedNotification(requesterId, currentUserId);
            _notificationRepository.Add(notification);
        }
        else if (status == FollowStatus.Rejected)
        {
            follow.RejectRequest();
        }
        
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<FollowResponseDto>(follow);
    }

    public async Task<List<FollowRequestDto>> GetFollowRequestsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var followRequests = await _followRepository.GetPendingFollowRequestsAsync(userId, page, pageSize);
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