using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories;

public class LikeRepository : GenericRepository<PostLike>, ILikeRepository
{
    public LikeRepository(InstagramDbContext context) : base(context)
    {
    }

    public async Task<PostLike?> GetLikeAsync(int postId, Guid userId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }

    public async Task<int> GetLikesCountAsync(int postId)
    {
        return await _context.Likes
            .CountAsync(l => l.PostId == postId);
    }

    public async Task<List<PostLike>> GetPostLikesAsync(int postId, int page = 1, int pageSize = 20)
    {
        return await _context.Likes
            .Where(l => l.PostId == postId)
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> IsPostLikedByUserAsync(int postId, Guid userId)
    {
        return await _context.Likes
            .AnyAsync(l => l.PostId == postId && l.UserId == userId);
    }
}
