using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;

public interface IMediaRepository : IGenericRepository<Media>
{
    Task<List<Media>> GetMediaByPostIdAsync(Guid postId);
    Task<List<Media>> GetMediaByMessageIdAsync(Guid messageId);
}