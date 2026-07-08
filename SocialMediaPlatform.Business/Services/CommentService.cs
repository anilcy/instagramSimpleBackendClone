using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.CommentDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Business.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommentService
            (ICommentRepository commentRepository, 
             INotificationRepository notificationRepository,
             IPostRepository postRepository,
             IUnitOfWork unitOfWork,
             IMapper mapper)
        {
            _commentRepository = commentRepository;
            _notificationRepository = notificationRepository;
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CommentDto> AddCommentAsync(CommentCreateDto dto, Guid userId)
        {
            
            var post = await _postRepository.GetByIdAsync(dto.PostId);
            if (post == null)                 // Check the post
                throw new KeyNotFoundException("Post not found");

            Comment? parent = null;
            if (dto.ParentCommentId.HasValue)  // Check the parent comment if it's a reply
            {
                parent = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value);
                if (parent == null || parent.PostId != dto.PostId)
                    throw new ArgumentException("Invalid parent comment");
            }
            
            var comment = new Comment(dto.PostId, userId, dto.Content, dto.ParentCommentId);
            _commentRepository.Add(comment);
            
            // After this point, it's all about notification creation, either a reply to a comment or a comment to a post
            
            if (parent != null)
            {
                if (parent.AuthorId != userId)
                {
                    var notification =
                        Notification.CommentReplyNotification(parent.AuthorId, userId, dto.PostId, comment.Id);
                    _notificationRepository.Add(notification);                     
                }
            }

            else
            {
                if (post.AuthorId != userId)
                {
                    var notification = Notification.CommentNotification(post.AuthorId, userId, comment.Id , dto.PostId);
                    _notificationRepository.Add(notification);
                }
            }
            
            await _unitOfWork.SaveChangesAsync(); // single transaction: comment + notification committed together
            return _mapper.Map<CommentDto>(comment);
        }


        public async Task<List<CommentDto>> GetCommentsByPostIdAsync(Guid postId, int page, int pageSize)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId, page, pageSize);
            return _mapper.Map<List<CommentDto>>(comments);
        }

        public async Task DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");
            if (comment.AuthorId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments.");
            comment.SoftDeleteComment();
            await _unitOfWork.SaveChangesAsync();
        }
    }
}