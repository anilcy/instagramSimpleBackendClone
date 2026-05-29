using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaPlatform.Data.Repositories;

public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(SocialMediaDbContext context) : base(context)
    {
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .Include(c => c.Author)
            .Include(c => c.Replies)                // alt yorumları al
                .ThenInclude(r => r.Author)         // alt yorum yazarlarını da al
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

}
