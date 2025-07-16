using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories;

public class LikeRepository : GenericRepository<PostLike>, ILikeRepository
{
    private readonly InstagramDbContext _context;

    public LikeRepository(InstagramDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> IsPostLikedByUserAsync(int postId, Guid userId)
    {
        return await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
    }
}
