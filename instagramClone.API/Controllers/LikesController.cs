using instagramClone.Entities.Dtos;

namespace instagramClone.API.Controllers;

using instagramClone.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class LikesController : BaseController
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(int postId)
    {
        var isLiked = await _likeService.ToggleLikeAsync(postId, CurrentUserId);
        return Ok(new { success = true, liked = isLiked });
    }
}
