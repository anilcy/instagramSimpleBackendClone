namespace instagramClone.API.Controllers;

using instagramClone.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using instagramClone.API.Extensions;

[Authorize]
[AutoRoute] // Will automatically become /api/v1/likes
[ApiController]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(int postId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var isLiked = await _likeService.ToggleLikeAsync(postId, userId);
        return Ok(new { success = true, liked = isLiked });
    }
}
