using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Dtos.MessageDtos;

namespace SocialMediaPlatform.API.Realtime;

[Authorize]
public class ChatHub : Hub
{
    private readonly IPresenceService _presence;
    private readonly IMessageService _messages;

    public ChatHub(IPresenceService presence, IMessageService messages)
    {
        _presence = presence;
        _messages = messages;
    }

    private Guid GetUserId()
    {
        var sub = Context.User?.FindFirst("sub")?.Value
                  ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(sub))
            throw new HubException("Unauthorized");
        return Guid.Parse(sub);
    }

    public override async Task OnConnectedAsync()
    {
        var me = GetUserId();
        await _presence.SetOnlineAsync(me, Context.ConnectionId);

        await Clients.User(me.ToString())
                     .SendAsync("presence:me", new { online = true, me });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var me = GetUserId();
        await _presence.SetOfflineAsync(me, Context.ConnectionId);

        var stillOnline = await _presence.IsOnlineAsync(me);
        if (!stillOnline)
        {
            var lastSeen = await _presence.GetLastSeenAsync(me);
            await Clients.User(me.ToString())
                         .SendAsync("presence:me", new { online = false, me, lastSeen });
        }

        await base.OnDisconnectedAsync(ex);
    }

    public async Task SendMessage(Guid toUserId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new HubException("Message cannot be empty.");

        var me = GetUserId();

        var dto = new MessageCreateDto
        {
            ReceiverId = toUserId,
            Content = text
        };

        MessageDto saved = await _messages.SendMessageAsync(me, dto);

        await Clients.User(toUserId.ToString()).SendAsync("dm:new", saved);
        await Clients.User(me.ToString()).SendAsync("dm:sent", saved);
    }

    public async Task Typing(Guid toUserId, bool isTyping)
    {
        var me = GetUserId();
        await Clients.User(toUserId.ToString())
                     .SendAsync("dm:typing", new { fromUserId = me, isTyping });
    }

    public async Task MarkConversationRead(Guid otherUserId)
    {
        var me = GetUserId();
        await _messages.MarkConversationAsReadAsync(me, otherUserId);

        await Clients.User(otherUserId.ToString())
                     .SendAsync("dm:read:conversation", new
                     {
                         readerId = me,
                         partnerId = otherUserId,
                         readAt = DateTime.UtcNow
                     });
    }

    public async Task<object> GetPresence(Guid userId)
    {
        var online = await _presence.IsOnlineAsync(userId);
        var lastSeen = await _presence.GetLastSeenAsync(userId);
        return new { online, lastSeen };
    }
}