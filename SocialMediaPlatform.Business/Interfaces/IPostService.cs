using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Dtos.PostDtos;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(PostCreateDto dto, Guid userId);
    Task<List<PostDto>> GetPostsAsync(Guid targetUserId, Guid requesterId, int page = 1, int pageSize = 20);
    Task<PostDto> GetPostByIdAsync(Guid postId);
    Task<List<PostDto>> GetFeedAsync(Guid userId, int page, int pageSize);
    Task UpdatePostAsync(Guid postId, PostUpdateDto dto, Guid userId);
    Task DeletePostAsync(Guid postId, Guid userId); // Soft delete
    Task LikePostAsync(Guid userId, Guid postId);
    Task UnlikePostAsync(Guid userId, Guid postId);
}
