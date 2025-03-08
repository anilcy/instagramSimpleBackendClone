using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly DbSet<Post> _dbSet;

        public PostRepository(InstagramDbContext context) : base(context)
        {
            _dbSet = context.Set<Post>();
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int page, int pageSize)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAndUserAsync(int postId, Guid userId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId && !p.IsDeleted);
        }
    }
}