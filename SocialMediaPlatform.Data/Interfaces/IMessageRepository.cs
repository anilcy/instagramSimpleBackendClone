using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Interfaces;

public interface IMessageRepository : IGenericRepository<Message>
{
    // all messages between two users
    Task<List<Message>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize);

    //all last messages between a user and all others that the user talked to
    Task<List<Message>> GetConversationsAsync(Guid userId, int page, int pageSize);

    // number of unread messages from a specific user
    Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid fromUserId);
    Task<List<Message>> GetUnreadFromUserAsync(Guid userId, Guid fromUserId);
    Task<Message?> GetByIdAsync(Guid messageId);

}