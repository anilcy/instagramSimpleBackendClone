using AutoMapper;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        public async Task<CommentDto> AddCommentAsync(CreateCommentDto dto, Guid userId)
        {
            // Eğer parentId verilmişse, geçerli bir parent mı kontrol et
            if (dto.ParentCommentId.HasValue)
            {
                var parent = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value);
                if (parent == null || parent.PostId != dto.PostId)
                    throw new ArgumentException("Invalid parent comment");
            }

            var comment = new Comment
            {
                PostId = dto.PostId,
                Content = dto.Content,
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = dto.ParentCommentId
            };

            await _commentRepository.InsertAsync(comment);
            await _commentRepository.SaveChangesAsync();

            return _mapper.Map<CommentDto>(comment);
        }


        public async Task<List<CommentDto>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
            return _mapper.Map<List<CommentDto>>(comments);
        }
    }
}