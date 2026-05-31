using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaPlatform.Data.Repositories;

public class LikeRepository : GenericRepository<PostLike>, ILikeRepository
{
    public LikeRepository(SocialMediaDbContext context) : base(context)
    {
    }

    public async Task<PostLike?> GetLikeAsync(Guid postId, Guid userId)
    {
        return await _context.Likes
            .SingleOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }

    public async Task<int> GetLikesCountAsync(Guid postId)
    {
        return await _context.Likes
            .CountAsync(l => l.PostId == postId);
    }

    public async Task<List<PostLike>> GetPostLikesAsync(Guid postId, int page = 1, int pageSize = 20)
    {
        return await _context.Likes
            .Where(l => l.PostId == postId)
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId)
    {
        return await _context.Likes
            .AnyAsync(l => l.PostId == postId && l.UserId == userId);
    }
}
