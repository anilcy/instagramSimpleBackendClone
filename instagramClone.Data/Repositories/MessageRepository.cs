using instagramClone.Data.Interfaces;
using instagramClone.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace instagramClone.Data.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(InstagramDbContext context) : base(context)
    {
    }

    public async Task<List<Message>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                       (m.SenderId == otherUserId && m.ReceiverId == userId))
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Message>> GetConversationsAsync(Guid userId)
    {
        var conversations = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.OrderByDescending(m => m.CreatedAt).First())
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return conversations;
    }

    public async Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid fromUserId)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && m.SenderId == fromUserId && !m.IsRead);
    }

    public async Task MarkMessagesAsReadAsync(Guid userId, Guid fromUserId)
    {
        var unreadMessages = await _context.Messages
            .Where(m => m.ReceiverId == userId && m.SenderId == fromUserId && !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Message?> GetLastMessageBetweenUsersAsync(Guid userId, Guid otherUserId)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                       (m.SenderId == otherUserId && m.ReceiverId == userId))
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
    }
}