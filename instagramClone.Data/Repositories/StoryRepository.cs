using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories;

public class StoryRepository : GenericRepository<Story>, IStoryRepository
{
    public StoryRepository(InstagramDbContext context) : base(context) { }

    public async Task<Story?> GetStoryAsync(int storyId)
    {
        return await _context.Stories
            .Include(s => s.User)
            .Include(s => s.Views)          // Story.Views → ICollection<StoryView>
            .FirstOrDefaultAsync(s => s.Id == storyId);
    }

    public async Task<List<Story>> GetUserActiveStoriesAsync(Guid userId,
                                                             int page = 1,
                                                             int pageSize = 20)
    {
        var now = DateTime.UtcNow;
        return await _context.Stories
            .Where(s => s.UserId == userId && s.ExpiresAt > now)
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Story>> GetStoriesFeedAsync(Guid userId,
        int page = 1,
        int pageSize = 20)
    {
        var now = DateTime.UtcNow;

        // IQueryable, henüz SQL’e çevrilmedi (deferred execution)
        var followingIds = _context.Follows
            .Where(f => f.FollowerId == userId && f.Status == FollowStatus.Accepted)
            .Select(f => f.FollowedId);

        return await _context.Stories
            .Where(s => s.ExpiresAt > now &&
                        (s.UserId == userId || followingIds.Contains(s.UserId)))
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }


    public async Task<int> GetStoryViewCountAsync(int storyId)
    {
        return await _context.StoryViews
            .CountAsync(v => v.StoryId == storyId);
    }

    public async Task<bool> HasUserViewedStoryAsync(int storyId, Guid userId)
    {
        return await _context.StoryViews
            .AnyAsync(v => v.StoryId == storyId && v.UserId == userId);
    }

    public async Task AddStoryViewAsync(StoryView view)
    {
        await _context.StoryViews.AddAsync(view);
    }

    public async Task<List<StoryView>> GetStoryViewsAsync(int storyId,
                                                          int page = 1,
                                                          int pageSize = 50)
    {
        return await _context.StoryViews
            .Where(v => v.StoryId == storyId)
            .Include(v => v.User)
            .OrderByDescending(v => v.ViewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
