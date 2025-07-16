using instagramClone.Entities.Models;

namespace instagramClone.Data.Interfaces;
public interface ILikeRepository : IGenericRepository<PostLike>
{
    Task<bool> IsPostLikedByUserAsync(int postId, Guid userId);
}