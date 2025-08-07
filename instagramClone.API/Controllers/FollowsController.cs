using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace instagramClone.API.Controllers;

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

    [HttpPost]
    public async Task<ActionResult<FollowDto>> FollowUser([FromBody] FollowActionDto followDto)
    {
        var currentUserId = CurrentUserId;
        var follow = await _followService.FollowUserAsync(currentUserId, followDto.TargetUserId);
        return Ok(follow);
    }

    [HttpDelete("{targetUserId:guid}")]
    public async Task<ActionResult> UnfollowUser(Guid targetUserId)
    {
        var currentUserId = CurrentUserId;
        var result = await _followService.UnfollowUserAsync(currentUserId, targetUserId);
        
        if (result)
            return NoContent();
        
        return NotFound("Follow relationship not found");
    }

    [HttpPost("requests/{requesterId:guid}/respond")]
    public async Task<ActionResult<FollowResponseDto>> RespondToFollowRequest(Guid requesterId, [FromBody] FollowStatus status)
    {
        var currentUserId = CurrentUserId;
        var response = await _followService.RespondToFollowRequestAsync(currentUserId, requesterId, status);
        return Ok(response);
    }

    [HttpGet("requests")]
    public async Task<ActionResult<List<FollowRequestDto>>> GetPendingFollowRequests()
    {
        var currentUserId = CurrentUserId;
        var requests = await _followService.GetFollowRequestsAsync(currentUserId);
        return Ok(requests);
    }

    [HttpGet("{userId:guid}/followers")]
    public async Task<ActionResult<List<UserSummaryDto>>> GetFollowers(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var followers = await _followService.GetFollowersAsync(userId, page, pageSize);
        return Ok(followers);
    }

    [HttpGet("{userId:guid}/following")]
    public async Task<ActionResult<List<UserSummaryDto>>> GetFollowing(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var following = await _followService.GetFollowingAsync(userId, page, pageSize);
        return Ok(following);
    }

    [HttpGet("{targetUserId:guid}/status")]
    public async Task<ActionResult<FollowStatus?>> GetFollowStatus(Guid targetUserId)
    {
        var currentUserId = CurrentUserId;
        var status = await _followService.GetFollowStatusAsync(currentUserId, targetUserId);
        return Ok(status);
    }
    
}