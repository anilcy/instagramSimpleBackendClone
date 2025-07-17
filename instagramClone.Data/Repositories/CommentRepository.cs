using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories;

public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(InstagramDbContext context) : base(context)
    {
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.Author)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}
