using System;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;
public interface ILikeRepository : IGenericRepository<PostLike>
{
    Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId);
}