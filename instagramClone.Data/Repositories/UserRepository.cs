using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories;

public class UserRepository : GenericRepository<AppUser>, IUserRepository
{
    public UserRepository(InstagramDbContext context) : base(context)
    {
    }

    public async Task<AppUser?> GetUserByUserNameAsync(string userName)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<AppUser>> SearchUsersAsync(string searchTerm, int page, int pageSize)
    {
        return await _context.Users
            .Where(u => (u.UserName != null && u.UserName.Contains(searchTerm)) || 
                       (u.FullName != null && u.FullName.Contains(searchTerm)))
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUserPostsCountAsync(Guid userId)
    {
        return await _context.Posts
            .CountAsync(p => p.AuthorId == userId);
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}