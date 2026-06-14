using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaPlatform.Data.Repositories;

public class StoryRepository : GenericRepository<Story>, IStoryRepository
{
    public StoryRepository(SocialMediaDbContext context) : base(context) { }

    public async Task<Story?> GetStoryAsync(Guid storyId)
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
            .Where(s => s.UserId == userId)
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
        var now = DateTimeOffset.UtcNow;

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


    public async Task<int> GetStoryViewCountAsync(Guid storyId)
    {
        return await _context.StoryViews
            .CountAsync(v => v.StoryId == storyId);
    }

    public async Task<bool> HasUserViewedStoryAsync(Guid storyId, Guid userId)
    {
        return await _context.StoryViews
            .AnyAsync(v => v.StoryId == storyId && v.UserId == userId);
    }

    public void AddStoryView(StoryView view)
    { 
        _context.StoryViews.AddAsync(view);
    }

    public void AddStoryLike(StoryLike like)
    {
        _context.StoryLikes.Add(like);
    }
    
    public async Task<List<StoryView>> GetStoryViewsAsync(Guid storyId,
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
    
    public async Task<StoryLike?> GetStoryLikeAsync(Guid storyId, Guid userId)
    {
        return await _context.StoryLikes
            .FirstOrDefaultAsync(sl => sl.StoryId == storyId && sl.UserId == userId);
    }
}
