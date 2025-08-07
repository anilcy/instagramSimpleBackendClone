using AutoMapper;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace instagramClone.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;

    public UserService(IUserRepository userRepository, IFollowRepository followRepository, 
                      IMapper mapper, UserManager<AppUser> userManager)
    {
        _userRepository = userRepository;
        _followRepository = followRepository;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<UserDto> GetUserProfileAsync(Guid userId, Guid? currentUserId = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        var userDto = _mapper.Map<UserDto>(user);
        
        // Get statistics
        userDto.PostsCount = await _userRepository.GetUserPostsCountAsync(userId);
        userDto.FollowersCount = await _followRepository.GetFollowersCountAsync(userId);
        userDto.FollowingCount = await _followRepository.GetFollowingCountAsync(userId);

        // If current user is provided, check relationship
        if (currentUserId.HasValue && currentUserId != userId)
        {
            userDto.IsFollowing = await _followRepository.IsFollowingAsync(currentUserId.Value, userId);
            var followRelation = await _followRepository.GetFollowRelationshipAsync(currentUserId.Value, userId);
            userDto.FollowStatus = followRelation?.Status;
        }

        return userDto;
    }

    public async Task<UserDto> GetUserByUserNameAsync(string userName, Guid? currentUserId = null)
    {
        var user = await _userRepository.GetUserByUserNameAsync(userName);
        if (user == null)
            throw new ArgumentException("User not found");

        return await GetUserProfileAsync(user.Id, currentUserId);
    }

    public async Task<UserDto> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        user.FullName = updateDto.FullName;
        user.Bio = updateDto.Bio;
        user.WebsiteUrl = updateDto.WebsiteUrl;
        user.IsPrivate = updateDto.IsPrivate;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return await GetUserProfileAsync(userId);
    }

    public async Task<List<UserSummaryDto>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        var users = await _userRepository.SearchUsersAsync(searchTerm, page, pageSize);
        return _mapper.Map<List<UserSummaryDto>>(users);
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        await _userRepository.UpdateLastLoginAsync(userId);
    }
}