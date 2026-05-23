using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;
public interface ICommentRepository : IGenericRepository<Comment>
{
     Task<List<Comment>> GetCommentsByPostIdAsync(int postId);
}
