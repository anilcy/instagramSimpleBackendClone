using instagramClone.Entities.Dtos;

namespace instagramClone.API.Controllers;

using instagramClone.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike([FromBody] LikeActionDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var isLiked = await _likeService.ToggleLikeAsync(dto.EntityId, userId);
        return Ok(new { success = true, liked = isLiked });
    }
}
