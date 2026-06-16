using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<List<Post>> GetPostsByUserIdAsync(Guid userId, int page, int pageSize);
        Task<Post?> GetPostByIdAsync(Guid postId);
        Task<List<Post>> GetFeedAsync(Guid userId, int page, int pageSize);
        Task<PostLike?> GetPostLikeAsync(Guid userId, Guid postId);
        void AddPostLike(PostLike like);
    }
}
