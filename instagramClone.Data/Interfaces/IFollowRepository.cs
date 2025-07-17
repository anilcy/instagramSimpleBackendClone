using instagramClone.Entities.Models;

namespace instagramClone.Data.Interfaces;

public interface IFollowRepository : IGenericRepository<Follow>
{
    Task<Follow?> GetFollowRelationshipAsync(Guid followerId, Guid followedId);
    Task<List<Follow>> GetFollowersAsync(Guid userId, int page, int pageSize);
    Task<List<Follow>> GetFollowingAsync(Guid userId, int page, int pageSize);
    Task<List<Follow>> GetPendingFollowRequestsAsync(Guid userId);
    Task<int> GetFollowersCountAsync(Guid userId);
    Task<int> GetFollowingCountAsync(Guid userId);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followedId);
}