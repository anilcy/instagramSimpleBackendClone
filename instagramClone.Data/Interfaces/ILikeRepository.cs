using instagramClone.Entities.Models;

namespace instagramClone.Data.Interfaces;

public interface ILikeRepository : IGenericRepository<Like>
{
    Task<bool> IsPostLikedByUserAsync(int postId, Guid userId);
}