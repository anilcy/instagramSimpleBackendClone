using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Dtos.MessageDtos;
using SocialMediaPlatform.Entities.Dtos.UserDtos;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Business.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;
    private readonly SocialMediaDbContext _dbContext;

    public MessageService(
        IMessageRepository messageRepository,
        INotificationRepository notificationRepository,
        IMapper mapper, 
        SocialMediaDbContext dbContext)
    {
        _messageRepository = messageRepository;
        _notificationRepository = notificationRepository;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, MessageCreateDto dto)
    {
        var message = new Message(senderId, dto.ReceiverId, dto.Content);
        _messageRepository.Add(message);

        var notification = Notification.MessageNotification(dto.ReceiverId, senderId);
        _notificationRepository.Add(notification);
        
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<MessageDto>(message);
    }

    public async Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page , int pageSize)
    {
        var message = await _messageRepository.GetConversationAsync(userId, otherUserId, page, pageSize);
        return _mapper.Map<List<MessageDto>>(message);
    }
    
    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId, int page, int pageSize)
    {
        var lastMessages = await _messageRepository.GetConversationsAsync(userId, page, pageSize);
        var conversations = new List<ConversationDto>();

        foreach (var msg in lastMessages)
        {
            var otherUser = msg.SenderId == userId ? msg.Receiver : msg.Sender;
            var unreadCount = await _messageRepository.GetUnreadMessagesCountAsync(userId, otherUser.Id);

            conversations.Add(new ConversationDto
            {
                OtherUser = _mapper.Map<UserSummaryDto>(otherUser),
                LastMessage = _mapper.Map<MessageDto>(msg),
                UnreadCount = unreadCount
            });
        }

        return conversations;
    }
    
    public async Task MarkConversationAsReadAsync(Guid userId, Guid fromUserId)
    {
        var unread = await _messageRepository.GetUnreadFromUserAsync(userId, fromUserId);
        foreach (var message in unread)
            message.MarkAsRead();

        await _dbContext.SaveChangesAsync();
    }
    
    public async Task MarkAsReadAsync(Guid messageId, Guid readerId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null) 
            throw new ArgumentException("Message is not found");
        if (message.ReceiverId != readerId)
            throw new ArgumentException("You can only mark your own messages as read.");

        message.MarkAsRead();   
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<int> GetUnreadCountAsync(Guid userId, Guid fromUserId)
    {
        return await _messageRepository.GetUnreadMessagesCountAsync(userId, fromUserId);
    }
    
    public async Task EditMessageAsync(Guid messageId, Guid userId, string newContent)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new ArgumentException("Message not found.");
        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("You can only edit your own messages.");

        message.EditMessage(newContent);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new ArgumentException("Message not found.");
        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("You can only delete your own messages.");

        message.SoftDeleteMessage();
        await _dbContext.SaveChangesAsync();
    }
}
