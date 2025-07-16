using instagramClone.Entities.Dtos;

namespace instagramClone.Business.Interfaces;

public interface IUserService
{
    Task<UserDto> GetUserProfileAsync(Guid userId, Guid? currentUserId = null);
    Task<UserDto> GetUserByUserNameAsync(string userName, Guid? currentUserId = null);
    Task<UserDto> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto updateDto);
    Task<List<UserSummaryDto>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 20);
    Task UpdateLastLoginAsync(Guid userId);
}