using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Repositories;

public class MediaRepository : GenericRepository<Media>, IMediaRepository
{
    public MediaRepository(SocialMediaDbContext context) : base(context) { }

    public async Task<List<Media>> GetMediaByPostIdAsync(Guid postId)
    {
        return await _context.MediaItems
            .Where(m => m.PostId == postId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Media>> GetMediaByMessageIdAsync(Guid messageId)
    {
        return await _context.MediaItems
            .Where(m => m.MessageId == messageId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }
}