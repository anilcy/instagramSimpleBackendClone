namespace instagramClone.Business.Interfaces;

public interface ILikeService
{
    Task<bool> ToggleLikeAsync(int postId, Guid userId);
}