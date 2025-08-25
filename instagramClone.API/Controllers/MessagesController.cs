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

    // HTTP ile mesaj gönderme (fallback veya ilk gönderim)
    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] CreateMessageDto messageDto)
    {
        if (messageDto == null || messageDto.ReceiverId == Guid.Empty || string.IsNullOrWhiteSpace(messageDto.Content))
            return BadRequest("ReceiverId and Content are required.");

        var message = await _messageService.SendMessageAsync(CurrentUserId, messageDto);
        return Ok(message);
    }

    // İki kullanıcı arasındaki konuşmayı getir (sayfalı)
    // GET /api/messages/conversations/{otherUserId}?page=1&pageSize=50
    [HttpGet("conversations/{otherUserId:guid}")]
    public async Task<ActionResult<List<MessageDto>>> GetConversation(
        Guid otherUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (otherUserId == Guid.Empty) return BadRequest("otherUserId is required.");

        var messages = await _messageService.GetConversationAsync(CurrentUserId, otherUserId, page, pageSize);
        return Ok(messages);
    }

    // Tek bir mesajı getir
    // GET /api/messages/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MessageDto?>> GetById(int id)
    {
        var message = await _messageService.GetByIdAsync(id);
        if (message == null) return NotFound();
        return Ok(message);
    }

    // Tek bir mesajı okundu işaretle
    // PUT /api/messages/{messageId}/read
    [HttpPut("{messageId:int}/read")]
    public async Task<ActionResult> MarkAsRead(int messageId)
    {
        var success = await _messageService.MarkAsReadAsync(messageId, CurrentUserId);
        if (!success) return NotFound();
        return NoContent();
    }

    // Konuşmadaki TÜM mesajları okundu işaretle (otherUser -> CurrentUserId)
    // PUT /api/messages/conversations/{otherUserId}/read
    [HttpPut("conversations/{otherUserId:guid}/read")]
    public async Task<ActionResult> MarkConversationAsRead(Guid otherUserId)
    {
        if (otherUserId == Guid.Empty) return BadRequest("otherUserId is required.");

        await _messageService.MarkMessagesAsReadAsync(CurrentUserId, otherUserId);
        return NoContent();
    }

    // Belirli bir kullanıcıdan gelen okunmamış mesaj sayısı
    // GET /api/messages/conversations/{otherUserId}/unread-count
    [HttpGet("conversations/{otherUserId:guid}/unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(Guid otherUserId)
    {
        if (otherUserId == Guid.Empty) return BadRequest("otherUserId is required.");

        var count = await _messageService.GetUnreadCountAsync(CurrentUserId, otherUserId);
        return Ok(count);
    }
}