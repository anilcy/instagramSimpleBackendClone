using instagramClone.Entities.Models;

namespace instagramClone.Data.Interfaces;
public interface ICommentRepository : IGenericRepository<Comment>
{
     Task<List<Comment>> GetCommentsByPostIdAsync(int postId);
}
