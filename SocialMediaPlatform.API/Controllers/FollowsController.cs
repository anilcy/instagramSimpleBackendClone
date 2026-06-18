using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SocialMediaPlatform.Entities.Dtos.FollowDtos;
using SocialMediaPlatform.Entities.Dtos.UserDtos;

namespace SocialMediaPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FollowsController : BaseController
{
    private readonly IFollowService _followService;

    public FollowsController(IFollowService followService)
    {
        _followService = followService;
    }

    [HttpPost("{targetUserId}")]
    public async Task<IActionResult> FollowUser(Guid targetUserId)
    {
        var follow = await _followService.FollowUserAsync(CurrentUserId, targetUserId);
        return Ok(follow);
    }

    [HttpDelete("{targetUserId}")]
    public async Task<IActionResult> UnfollowUser(Guid targetUserId)
    {

        var result = await _followService.UnfollowUserAsync(CurrentUserId, targetUserId);
        
        if (result)
            return NoContent();
        
        return NotFound("Follow relationship not found");
    }

    [HttpPost("requests/{requesterId}/accept")]
    public async Task<IActionResult> AcceptFollowRequest(Guid requesterId)
    {
        var follow = await _followService.RespondToFollowRequestAsync(CurrentUserId, requesterId, FollowStatus.Accepted);
        return Ok(follow);
    }

    [HttpPost("requests/{requesterId}/reject")]
    public async Task<IActionResult> RejectFollowRequest(Guid requesterId)
    {
        var follow = await _followService.RespondToFollowRequestAsync(CurrentUserId, requesterId, FollowStatus.Rejected);
        return Ok(follow);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetPendingFollowRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var requests = await _followService.GetFollowRequestsAsync(CurrentUserId, page, pageSize);
        return Ok(requests);
    }

    [HttpGet("{userId}/followers")]
    public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var followers = await _followService.GetFollowersAsync(userId, page, pageSize);
        return Ok(followers);
    }

    [HttpGet("{userId}/following")]
    public async Task<IActionResult> GetFollowing(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var following = await _followService.GetFollowingAsync(userId, page, pageSize);
        return Ok(following);
    }

    [HttpGet("{targetUserId}/status")]
    public async Task<IActionResult> GetFollowStatus(Guid targetUserId)
    {
        var status = await _followService.GetFollowStatusAsync(CurrentUserId, targetUserId);
        return Ok(status);
    }
    
}