using AutoMapper;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;
using instagramClone.Data.Repositories;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace instagramClone.Business.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public PostService(IPostRepository postRepository, 
                           IFileStorageService fileStorageService, 
                           IMapper mapper,
                           UserManager<AppUser> userManager)
        {
            _postRepository = postRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
            _userManager = userManager; 
        }

        public async Task<PostDto> CreatePostAsync(PostCreateDto dto, Guid userId)
        {
            // Dosya yükleme
            string uploadedImageUrl = await _fileStorageService.UploadFileAsync(dto.ImageFile);

            // Yeni Post oluşturma
            var post = new Post
            {
                Caption = dto.Caption,
                ImageUrl = uploadedImageUrl,
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            // Repository üzerinden ekleyip kaydediyoruz
            await _postRepository.InsertAsync(post);
            await _postRepository.SaveChangesAsync();

            // Get user for Author mapping
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                post.Author = user;
            }

            return _mapper.Map<PostDto>(post);
        }

        public async Task<List<PostDto>> GetPostsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId, page, pageSize);
            return _mapper.Map<List<PostDto>>(posts);
        }

        public async Task<PostDto> GetPostByIdAsync(int postId, Guid userId)
        {
            var post = await _postRepository.GetPostByIdAndUserAsync(postId, userId);
            if (post == null)
                throw new Exception("Post not found.");

            return _mapper.Map<PostDto>(post);
        }

        public async Task<PostDto> UpdatePostAsync(int postId, PostUpdateDto dto, Guid userId)
        {
            var post = await _postRepository.GetPostByIdAndUserAsync(postId, userId);
            if (post == null)
                throw new Exception("Post not found or not accessible.");

            // 📌 Eğer yeni bir dosya yollandıysa (Zorunlu değil, opsiyonel hale getirdik)
            if (dto.ImageFile != null)
            {
                // 📌 Eğer postun eski bir resmi varsa, onu sil
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(post.ImageUrl); // ✅ Yeni eklenen işlem
                }

                // 📌 Yeni resmi yükleyip, post'a kaydet
                string newImageUrl = await _fileStorageService.UploadFileAsync(dto.ImageFile);
                post.ImageUrl = newImageUrl;
            }

            // 📌 Caption güncelleme (Zorunlu değil)
            if (!string.IsNullOrEmpty(dto.Caption))
                post.Caption = dto.Caption;

            post.ModifiedAt = DateTime.UtcNow;

            // 📌 Repository üzerinden update
            await _postRepository.UpdateAsync(post);

            // 📌 Eğer UpdateAsync içinde zaten SaveChanges çağrılıyorsa, buradaki satır gereksiz.
            // await _postRepository.SaveChangesAsync(); ❌ Kaldırıldı

            return _mapper.Map<PostDto>(post);
        }


        public async Task<bool> DeletePostAsync(int postId, Guid userId)
        {
            var post = await _postRepository.GetPostByIdAndUserAsync(postId, userId);
            if (post == null)
                return false;

            // Soft delete
            post.IsDeleted = true;
            post.DeletedAt = DateTime.UtcNow;

            await _postRepository.UpdateAsync(post);
            await _postRepository.SaveChangesAsync();

            return true;
        }
    }
}
