using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        public PostRepository(InstagramDbContext context) : base(context)
        {
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int page, int pageSize)
        {
            return await _context.Posts
                .Where(p => p.AuthorId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAndUserAsync(int postId, Guid userId)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId && p.AuthorId == userId && !p.IsDeleted);
        }
    }
}