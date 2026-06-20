using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaPlatform.Entities.Dtos.MessageDtos;

namespace SocialMediaPlatform.API.Controllers;

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
    public async Task<IActionResult> SendMessage([FromBody] MessageCreateDto dto)
    {
        var message = await _messageService.SendMessageAsync(CurrentUserId, dto);
        return Ok(message);
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var conversations = await _messageService.GetConversationsAsync(CurrentUserId, page, pageSize);
        return Ok(conversations);
    }

    [HttpGet("conversations/{otherUserId:guid}")]
    public async Task<IActionResult> GetConversation(Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.GetConversationAsync(CurrentUserId, otherUserId, page, pageSize);
        return Ok(messages);
    }

    [HttpPut("{messageId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        await _messageService.MarkAsReadAsync(messageId, CurrentUserId);
        return NoContent();
    }

    [HttpPut("conversations/{otherUserId:guid}/read")]
    public async Task<IActionResult> MarkConversationAsRead(Guid otherUserId)
    {
        await _messageService.MarkConversationAsReadAsync(CurrentUserId, otherUserId);
        return NoContent();
    }

    [HttpGet("conversations/{otherUserId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid otherUserId)
    {
        var count = await _messageService.GetUnreadCountAsync(CurrentUserId, otherUserId);
        return Ok(count);
    }

    [HttpPut("{messageId:guid}")]
    public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] string newContent)
    {
        await _messageService.EditMessageAsync(messageId, CurrentUserId, newContent);
        return NoContent();
    }

    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        await _messageService.DeleteMessageAsync(messageId, CurrentUserId);
        return NoContent();
    }
}