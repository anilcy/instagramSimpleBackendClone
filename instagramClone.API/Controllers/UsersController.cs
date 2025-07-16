using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace instagramClone.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserDto>> GetUserProfile(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var user = await _userService.GetUserProfileAsync(userId, currentUserId);
        return Ok(user);
    }

    [HttpGet("username/{userName}")]
    public async Task<ActionResult<UserDto>> GetUserByUserName(string userName)
    {
        var currentUserId = GetCurrentUserId();
        var user = await _userService.GetUserByUserNameAsync(userName, currentUserId);
        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMyProfile()
    {
        var currentUserId = GetCurrentUserId();
        var user = await _userService.GetUserProfileAsync(currentUserId);
        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMyProfile([FromBody] UpdateUserProfileDto updateDto)
    {
        var currentUserId = GetCurrentUserId();
        var user = await _userService.UpdateUserProfileAsync(currentUserId, updateDto);
        return Ok(user);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<UserSummaryDto>>> SearchUsers([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _userService.SearchUsersAsync(searchTerm, page, pageSize);
        return Ok(users);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}