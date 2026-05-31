using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int page, int pageSize);
        Task<Post?> GetPostByIdAndUserAsync(int postId, Guid userId);
    }
}
