using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Entities.Dtos.Story;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaPlatform.Entities.Dtos.StoryDtos;

namespace SocialMediaPlatform.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StoriesController : BaseController
{
    private readonly IStoryService _storyService;

    public StoriesController(IStoryService storyService)
    {
        _storyService = storyService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateStory([FromForm] IFormFile mediaFile)
    {
        var story = await _storyService.CreateStoryAsync(CurrentUserId, mediaFile);
        return Ok(story);
    }

    [AllowAnonymous]
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserStories(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var stories = await _storyService.GetUserActiveStoriesAsync(userId, CurrentUserIdOrNull, page, pageSize);
        return Ok(stories);
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var stories = await _storyService.GetStoriesFeedAsync(CurrentUserId, page, pageSize);
        return Ok(stories);
    }

    [HttpPost("{storyId:guid}/view")]
    public async Task<IActionResult> ViewStory(Guid storyId)
    {
        await _storyService.AddStoryViewAsync(storyId, CurrentUserId);
        return Ok();
    }

    [HttpPost("{storyId:guid}/like")]
    public async Task<IActionResult> LikeStory(Guid storyId)
    {
        await _storyService.LikeStoryAsync(CurrentUserId, storyId);
        return Ok();
    }

    [HttpDelete("{storyId:guid}/like")]
    public async Task<IActionResult> UnlikeStory(Guid storyId)
    {
        await _storyService.UnlikeStoryAsync(CurrentUserId, storyId);
        return NoContent();
    }

    [HttpDelete("{storyId:guid}")]
    public async Task<IActionResult> DeleteStory(Guid storyId)
    {
        await _storyService.DeleteStoryAsync(storyId, CurrentUserId);
        return NoContent();
    }
}
