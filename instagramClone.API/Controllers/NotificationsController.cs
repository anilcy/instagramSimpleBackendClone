using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace instagramClone.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentUserId = GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(currentUserId, page, pageSize);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadNotificationsCount()
    {
        var currentUserId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadNotificationsCountAsync(currentUserId);
        return Ok(count);
    }

    [HttpPut("{notificationId:int}/read")]
    public async Task<ActionResult> MarkNotificationAsRead(int notificationId)
    {
        var currentUserId = GetCurrentUserId();
        await _notificationService.MarkNotificationAsReadAsync(notificationId, currentUserId);
        return NoContent();
    }

    [HttpPut("mark-all-read")]
    public async Task<ActionResult> MarkAllNotificationsAsRead()
    {
        var currentUserId = GetCurrentUserId();
        await _notificationService.MarkAllNotificationsAsReadAsync(currentUserId);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}