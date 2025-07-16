using instagramClone.Entities.Models;

namespace instagramClone.Data.Interfaces;

public interface IUserRepository : IGenericRepository<AppUser>
{
    Task<AppUser?> GetUserByUserNameAsync(string userName);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<List<AppUser>> SearchUsersAsync(string searchTerm, int page, int pageSize);
    Task<int> GetUserPostsCountAsync(Guid userId);
    Task UpdateLastLoginAsync(Guid userId);
}