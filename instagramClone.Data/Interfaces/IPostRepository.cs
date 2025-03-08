using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;


namespace instagramClone.Data.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int page, int pageSize);
        Task<Post?> GetPostByIdAndUserAsync(int postId, Guid userId);
    }
}
