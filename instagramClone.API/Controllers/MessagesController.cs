using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace instagramClone.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : BaseController
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] CreateMessageDto messageDto)
    {
        var currentUserId = GetCurrentUserId();
        var message = await _messageService.SendMessageAsync(currentUserId, messageDto);
        return Ok(message);
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations()
    {
        var currentUserId = GetCurrentUserId();
        var conversations = await _messageService.GetConversationsAsync(currentUserId);
        return Ok(conversations);
    }

    [HttpGet("conversations/{otherUserId:guid}")]
    public async Task<ActionResult<List<MessageDto>>> GetConversation(Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var currentUserId = GetCurrentUserId();
        var messages = await _messageService.GetConversationAsync(currentUserId, otherUserId, page, pageSize);
        return Ok(messages);
    }

    [HttpPut("conversations/{otherUserId:guid}/read")]
    public async Task<ActionResult> MarkMessagesAsRead(Guid otherUserId)
    {
        var currentUserId = GetCurrentUserId();
        await _messageService.MarkMessagesAsReadAsync(currentUserId, otherUserId);
        return NoContent();
    }

    [HttpGet("conversations/{otherUserId:guid}/unread-count")]
    public async Task<ActionResult<int>> GetUnreadMessagesCount(Guid otherUserId)
    {
        var currentUserId = GetCurrentUserId();
        var count = await _messageService.GetUnreadMessagesCountAsync(currentUserId, otherUserId);
        return Ok(count);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}