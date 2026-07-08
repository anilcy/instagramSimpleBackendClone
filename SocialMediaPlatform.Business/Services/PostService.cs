using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;
using Microsoft.AspNetCore.Identity;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Entities.Dtos.PostDtos;

namespace SocialMediaPlatform.Business.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMediaRepository _mediaRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IPrivacyService _privacyService;
        private readonly IUnitOfWork _unitOfWork;

        public PostService(IPostRepository postRepository,
                           IFileStorageService fileStorageService,
                           IMediaRepository mediaRepository,
                           INotificationRepository notificationRepository,
                           IMapper mapper,
                           IPrivacyService privacyService,
                           IUnitOfWork unitOfWork)
        {
            _postRepository = postRepository;
            _fileStorageService = fileStorageService;
            _mediaRepository = mediaRepository;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _privacyService = privacyService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PostDto> CreatePostAsync(PostCreateDto dto, Guid userId)
        {
            
            var post = new Post(userId, dto.Caption);
            _postRepository.Add(post);
            
            foreach (var file in dto.MediaFiles)
            {
                var url = await _fileStorageService.UploadFileAsync(file);
                var media = Media.ForPost(userId, post.Id, url, MediaType.Image);
                _mediaRepository.Add(media);
            }
          
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PostDto>(post);
        }

        public async Task<List<PostDto>> GetPostsAsync(Guid targetUserId, Guid requesterId, int page, int pageSize)
        {
            await _privacyService.EnsureCanAccessAsync(targetUserId, requesterId);
            var posts = await _postRepository.GetPostsByUserIdAsync(targetUserId, page, pageSize);
            return _mapper.Map<List<PostDto>>(posts);
        }

        public async Task<PostDto> GetPostByIdAsync(Guid postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            return _mapper.Map<PostDto>(post);
        }
        
        public async Task<List<PostDto>> GetFeedAsync(Guid userId, int page, int pageSize)
        {
            var posts = await _postRepository.GetFeedAsync(userId, page, pageSize);
            return _mapper.Map<List<PostDto>>(posts);
        }


        public async Task UpdatePostAsync(Guid postId, PostUpdateDto dto, Guid userId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
                throw new KeyNotFoundException("Post not found.");
            if (post.AuthorId != userId)
                throw new UnauthorizedAccessException("You can only edit your own posts.");
            
            post.UpdatePost(dto.Caption);
            await _unitOfWork.SaveChangesAsync();
        }


        public async Task DeletePostAsync(Guid postId, Guid userId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
                throw new KeyNotFoundException("Post not found.");
            if (post.AuthorId != userId)
                throw new UnauthorizedAccessException("You can only delete your own posts.");
            
            post.SoftDeletePost();
            await _unitOfWork.SaveChangesAsync();
        }
        
        public async Task LikePostAsync(Guid userId, Guid postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            var existingLike = await _postRepository.GetPostLikeAsync(userId, postId);
            if (existingLike != null)
                throw new InvalidOperationException("Already liked.");

            var like = new PostLike(userId, postId);
            _postRepository.AddPostLike(like);

            if (post.AuthorId != userId)
            {
                var notification = Notification.PostLikeNotification(post.AuthorId, userId, postId);
                _notificationRepository.Add(notification);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UnlikePostAsync(Guid userId, Guid postId)
        {
            var like = await _postRepository.GetPostLikeAsync(userId, postId);
            if (like == null)
                throw new KeyNotFoundException("Like not found.");

            like.SoftDeletePostLike();
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
