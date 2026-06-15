using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Dtos;
using SocialMediaPlatform.Entities.Dtos.MessageDtos;

namespace SocialMediaPlatform.Business.Interfaces;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(Guid senderId, MessageCreateDto dto);
    Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize);
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId, int page, int pageSize);
    Task MarkConversationAsReadAsync(Guid userId, Guid fromUserId);
    Task MarkAsReadAsync(Guid messageId, Guid readerId);
    Task<int> GetUnreadCountAsync(Guid userId, Guid fromUserId);
    Task EditMessageAsync(Guid messageId, Guid userId, string newContent);
    Task DeleteMessageAsync(Guid messageId, Guid userId);
}