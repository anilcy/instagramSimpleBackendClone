using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    
    public MessageRepository(SocialMediaDbContext context) : base(context)
    {
    }

    // all messages between two users
    public async Task<List<Message>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;

        return await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || 
                                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.CreatedAt)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    //all last messages between a user and all others that the user talked to
    public async Task<List<Message>> GetConversationsAsync(Guid userId, int page, int pageSize)
    {
        var messages = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.First())
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    // number of unread messages from a specific user
    public Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid fromUserId)
    {
        return _context.Messages
            .CountAsync(m => m.ReceiverId == userId && m.SenderId == fromUserId && !m.IsRead);
    }
    
    // MessageRepository
    public async Task<List<Message>> GetUnreadFromUserAsync(Guid userId, Guid fromUserId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId == userId && m.SenderId == fromUserId && !m.IsRead)
            .ToListAsync();
    }

    public async Task<Message?> GetByIdAsync(Guid messageId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }
    
}