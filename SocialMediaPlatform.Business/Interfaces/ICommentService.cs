using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Dtos.CommentDtos;


namespace SocialMediaPlatform.Business.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> AddCommentAsync(CommentCreateDto dto, Guid userId);
        Task<List<CommentDto>> GetCommentsByPostIdAsync(Guid postId, int page, int pageSize);
        Task DeleteCommentAsync(Guid commentId, Guid userId);
    }
}