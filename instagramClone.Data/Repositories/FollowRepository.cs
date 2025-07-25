using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories;

public class FollowRepository : GenericRepository<Follow>, IFollowRepository
{
    private readonly DbSet<Follow> _dbSet;

    public FollowRepository(InstagramDbContext context) : base(context)
    {
        _dbSet = context.Set<Follow>();
    }

    public async Task<Follow?> GetFollowRelationshipAsync(Guid followerId, Guid followedId)
    {
        return await _dbSet
            .Include(f => f.Follower)
            .Include(f => f.Followed)
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
    }

    public async Task<List<Follow>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        return await _dbSet
            .Where(f => f.FollowedId == userId && f.Status == FollowStatus.Accepted)
            .Include(f => f.Follower)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Follow>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        return await _dbSet
            .Where(f => f.FollowerId == userId && f.Status == FollowStatus.Accepted)
            .Include(f => f.Followed)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Follow>> GetPendingFollowRequestsAsync(Guid userId)
    {
        return await _dbSet
            .Where(f => f.FollowedId == userId && f.Status == FollowStatus.Pending)
            .Include(f => f.Follower)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetFollowersCountAsync(Guid userId)
    {
        return await _dbSet
            .CountAsync(f => f.FollowedId == userId && f.Status == FollowStatus.Accepted);
    }

    public async Task<int> GetFollowingCountAsync(Guid userId)
    {
        return await _dbSet
            .CountAsync(f => f.FollowerId == userId && f.Status == FollowStatus.Accepted);
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followedId)
    {
        return await _dbSet
            .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId 
                          && f.Status == FollowStatus.Accepted);
    }
}