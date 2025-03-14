using AutoMapper;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;

namespace instagramClone.Business.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public LikeService(ILikeRepository likeRepository, IPostRepository postRepository, IMapper mapper)
    {
        _likeRepository = likeRepository;
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<bool> ToggleLikeAsync(int postId, Guid userId)
    {
        var isLiked = await _likeRepository.IsPostLikedByUserAsync(postId, userId);
        if (isLiked)
        {
            var like = (await _likeRepository.FindAsync(l => l.PostId == postId && l.UserId == userId)).FirstOrDefault();
            if (like != null)
            {
                await _likeRepository.DeleteAsync(like);
                await _likeRepository.SaveChangesAsync();
            }
            return false; // Unlike işlemi gerçekleşti
        }
        else
        {
            var like = new Like { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow };
            await _likeRepository.InsertAsync(like);
            await _likeRepository.SaveChangesAsync();
            return true; // Like işlemi gerçekleşti
        }
    }
}
