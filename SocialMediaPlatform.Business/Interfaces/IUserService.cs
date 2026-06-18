using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IUserService
{
    Task<UserDto> GetUserProfileAsync(Guid userId, Guid? currentUserId = null);
    Task<UserDto> GetUserByUserNameAsync(string userName, Guid? currentUserId = null);
    Task<UserDto> UpdateUserProfileAsync(Guid userId, UserProfileUpdateDto userProfileUpdateDto);
    Task<List<UserSummaryDto>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 20);
    Task UpdateLastLoginAsync(Guid userId);
    Task SetPrivacyAsync(Guid userId, bool isPrivate);
    Task DeactivateAccountAsync(Guid userId);
    Task SoftDeleteAccountAsync(Guid userId);
}