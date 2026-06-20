using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Entities.Dtos.PostDtos;

namespace SocialMediaPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : BaseController
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] PostCreateDto dto)
    {
        var result = await _postService.CreatePostAsync(dto, CurrentUserId);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPosts(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var posts = await _postService.GetPostsAsync(userId, CurrentUserIdOrNull ?? Guid.Empty, page, pageSize);
        return Ok(posts);
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var posts = await _postService.GetFeedAsync(CurrentUserId, page, pageSize);
        return Ok(posts);
    }

    [AllowAnonymous]
    [HttpGet("{postId}")]
    public async Task<IActionResult> GetPostById(Guid postId)
    {
        var post = await _postService.GetPostByIdAsync(postId);
        return Ok(post);
    }

    [HttpPut("{postId}")]
    public async Task<IActionResult> UpdatePost(Guid postId, [FromBody] PostUpdateDto dto)
    {
        await _postService.UpdatePostAsync(postId, dto, CurrentUserId);
        return NoContent();
    }

    [HttpDelete("{postId}")]
    public async Task<IActionResult> DeletePost(Guid postId)
    {
        await _postService.DeletePostAsync(postId, CurrentUserId);
        return NoContent();
    }

    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        await _postService.LikePostAsync(CurrentUserId, postId);
        return Ok();
    }

    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> UnlikePost(Guid postId)
    {
        await _postService.UnlikePostAsync(CurrentUserId, postId);
        return NoContent();
    }
}
