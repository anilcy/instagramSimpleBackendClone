using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaPlatform.Data.Repositories;

public class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(SocialMediaDbContext context) : base(context)
    {
    }

    public async Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int page, int pageSize)
    {
        return await _context.Posts
            .Where(p => p.AuthorId ==
                        userId) // no need to check !p.IsDeleted, global query in dbContext does it already.
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAndUserAsync(Guid postId, Guid userId)
    {
        return await _context.Posts
            .Include(p => p.MediaItems)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == postId && p.AuthorId == userId && !p.IsDeleted);
    }

    public async Task<Post?> GetPostByIdAsync(Guid postId)
    {
        return await _context.Posts
            .Include(p => p.MediaItems)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == postId);

    }

    public async Task<List<Post>> GetFeedAsync(Guid userId, int page, int pageSize)
    {
        var followingIds = _context.Follows
            .Where(f => f.FollowerId == userId && f.Status == FollowStatus.Accepted)
            .Select(f => f.FollowedId);

        return await _context.Posts
            .Where(p => (p.AuthorId == userId || followingIds.Contains(p.AuthorId)))
            .Include(p => p.MediaItems)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .ThenInclude(c => c.Author)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<PostLike?> GetPostLikeAsync(Guid userId, Guid postId)
    {
        return await _context.PostLikes
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PostId == postId);
    }
    
    public void AddPostLike(PostLike like)
    {
        _context.PostLikes.Add(like);
    }
}
