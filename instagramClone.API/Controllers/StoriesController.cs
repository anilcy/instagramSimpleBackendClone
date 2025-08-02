using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos.Story;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace instagramClone.API.Controllers;

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

    // Yardımcı fonksiyon: token’dan userId çek
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    //Yeni hikâye ekle
    [HttpPost]
    public async Task<IActionResult> CreateStory([FromBody] StoryCreateDto dto)
    {
        var story = await _storyService.CreateStoryAsync(CurrentUserId, dto);
        return CreatedAtAction(nameof(GetStory), new { id = story.Id }, story);
    }

    //Tek hikâye getir (gerekirse)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStory(int id)
    {
        // Basit örnek: repo’dan çağırabilirsin
        return Ok(); // detaylı implement’e şu an gerek yok
    }

    //Kullanıcının aktif hikâyeleri
    [HttpGet("user/{userId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserStories(Guid userId, int page = 1, int pageSize = 20)
    {
        var stories = await _storyService.GetUserActiveStoriesAsync(userId, page, pageSize);
        return Ok(stories);
    }

    //Takip + kendi feed
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed(int page = 1, int pageSize = 20)
    {
        var stories = await _storyService.GetStoriesFeedAsync(CurrentUserId, page, pageSize);
        return Ok(stories);
    }

    //Hikâyeyi izledim
    [HttpPost("{id:int}/view")]
    public async Task<IActionResult> ViewStory(int id)
    {
        var added = await _storyService.AddStoryViewAsync(id, CurrentUserId);
        return added ? Ok() : BadRequest("Story already viewed.");
    }
}
