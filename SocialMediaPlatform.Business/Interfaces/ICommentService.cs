using SocialMediaPlatform.Entities.Dtos;


namespace SocialMediaPlatform.Business.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> AddCommentAsync(CreateCommentDto dto, Guid userId);
        Task<List<CommentDto>> GetCommentsByPostIdAsync(int postId);
    }
}