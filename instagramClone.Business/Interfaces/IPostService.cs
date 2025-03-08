using instagramClone.Entities.Dtos;

namespace instagramClone.Business.Interfaces;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(PostCreateDto dto, Guid userId);
    Task<List<PostDto>> GetPostsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<PostDto> GetPostByIdAsync(int postId, Guid userId);
    Task<PostDto> UpdatePostAsync(int postId, PostUpdateDto dto, Guid userId);
    Task<bool> DeletePostAsync(int postId, Guid userId); // Soft delete
}
