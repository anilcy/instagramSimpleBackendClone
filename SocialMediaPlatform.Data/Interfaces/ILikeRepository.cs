using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;
public interface ILikeRepository : IGenericRepository<PostLike>
{
    Task<bool> IsPostLikedByUserAsync(int postId, Guid userId);
}