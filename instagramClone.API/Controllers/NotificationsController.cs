using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var notifications = await _notificationService.GetUserNotificationsAsync(CurrentUserId, page, pageSize);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadNotificationsCount()
    {
        var count = await _notificationService.GetUnreadNotificationsCountAsync(CurrentUserId);
        return Ok(count);
    }

    [HttpPut("{notificationId:int}/read")]
    public async Task<ActionResult> MarkNotificationAsRead(int notificationId)
    {
        await _notificationService.MarkNotificationAsReadAsync(notificationId, CurrentUserId);
        return NoContent();
    }

    [HttpPut("mark-all-read")]
    public async Task<ActionResult> MarkAllNotificationsAsRead()
    {
        await _notificationService.MarkAllNotificationsAsReadAsync(CurrentUserId);
        return NoContent();
    }
    
}