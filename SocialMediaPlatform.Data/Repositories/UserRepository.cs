using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaPlatform.Data.Repositories;

public class UserRepository : GenericRepository<AppUser>, IUserRepository
{
    public UserRepository(SocialMediaDbContext context) : base(context)
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
        // ILIKE: Postgres's case-insensitive LIKE. Contains() translates to LIKE,
        // which is case-SENSITIVE in Postgres — searching for "ali" wasn't finding "Ali_Yilmaz"
        // (UserSearchIntegrationTests caught this issue).
        var pattern = $"%{searchTerm}%";
        return await _context.Users
            .Where(u => (u.UserName != null && EF.Functions.ILike(u.UserName, pattern)) ||
                        (u.FullName != null && EF.Functions.ILike(u.FullName, pattern)))
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
    
}