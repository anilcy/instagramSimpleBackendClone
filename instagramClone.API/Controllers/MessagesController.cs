using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
        var message = await _messageService.SendMessageAsync(CurrentUserId, messageDto);
        return Ok(message);
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations()
    {
        var conversations = await _messageService.GetConversationsAsync(CurrentUserId);
        return Ok(conversations);
    }

    [HttpGet("conversations/{otherUserId:guid}")]
    public async Task<ActionResult<List<MessageDto>>> GetConversation(Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.GetConversationAsync(CurrentUserId, otherUserId, page, pageSize);
        return Ok(messages);
    }

    [HttpPut("conversations/{otherUserId:guid}/read")]
    public async Task<ActionResult> MarkMessagesAsRead(Guid otherUserId)
    {
        await _messageService.MarkMessagesAsReadAsync(CurrentUserId, otherUserId);
        return NoContent();
    }

    [HttpGet("conversations/{otherUserId:guid}/unread-count")]
    public async Task<ActionResult<int>> GetUnreadMessagesCount(Guid otherUserId)
    {
        var count = await _messageService.GetUnreadMessagesCountAsync(CurrentUserId, otherUserId);
        return Ok(count);
    }
    
}