using Microsoft.EntityFrameworkCore;
using SocialMediaPlatform.Data.Interfaces;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Data.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    // DbContext'i base sınıfa da geçiyoruz
    private readonly SocialMediaDbContext _db;

    public MessageRepository(SocialMediaDbContext context) : base(context)
    {
        _db = context;
    }

    // İki kullanıcı arasındaki tüm mesajlar (kronolojik, sayfalı)
    public async Task<List<Message>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;

        return await _db.Messages
            .Where(m => !m.IsDeleted &&
                        ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                         (m.SenderId == otherUserId && m.ReceiverId == userId)))
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .ToListAsync();
    }

    // Kullanıcının tüm “karşı taraf”larla olan konuşmalarından her biri için “son mesajı” döndürür
    // Not: Basit çözüm — performansı artırmak için projection/Grouping yaptık.
    public async Task<List<Message>> GetConversationsAsync(Guid userId)
    {
        var query = _db.Messages
            .Where(m => !m.IsDeleted &&
                        (m.SenderId == userId || m.ReceiverId == userId));

        // Konuşma anahtarı: her iki yönde de aynı key olsun diye ordered pair
        var conv = await query
            .Select(m => new
            {
                Message = m,
                OtherId = m.SenderId == userId ? m.ReceiverId : m.SenderId,
                PairA = m.SenderId.CompareTo(m.ReceiverId) <= 0 ? m.SenderId : m.ReceiverId,
                PairB = m.SenderId.CompareTo(m.ReceiverId) <= 0 ? m.ReceiverId : m.SenderId
            })
            .GroupBy(x => new { x.PairA, x.PairB })
            .Select(g => g.OrderByDescending(x => x.Message.CreatedAt).First().Message)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return conv;
    }

    // Belirli bir karşı taraftan gelen okunmamış mesaj sayısı
    public Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid fromUserId)
    {
        return _db.Messages.CountAsync(m =>
            !m.IsDeleted &&
            m.ReceiverId == userId &&
            m.SenderId == fromUserId &&
            !m.IsRead);
    }

    // Konuşma bazlı "okundu" işaretleme (fromUserId -> userId)
    public async Task MarkMessagesAsReadAsync(Guid userId, Guid fromUserId)
    {
        // EF Core 9: set-based update ile hızlı işaretleme
        try
        {
            await _db.Messages
                .Where(m => !m.IsDeleted &&
                            m.ReceiverId == userId &&
                            m.SenderId == fromUserId &&
                            !m.IsRead)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.IsRead, true)
                    .SetProperty(m => m.ReadAt, DateTime.UtcNow));
        }
        catch (NotSupportedException)
        {
            // Sağlayıcı ExecuteUpdateAsync desteklemiyorsa fallback
            var list = await _db.Messages
                .Where(m => !m.IsDeleted &&
                            m.ReceiverId == userId &&
                            m.SenderId == fromUserId &&
                            !m.IsRead)
                .ToListAsync();

            foreach (var m in list)
            {
                m.IsRead = true;
                m.ReadAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }

    // İki kullanıcı arasındaki son mesaj
    public Task<Message?> GetLastMessageBetweenUsersAsync(Guid userId, Guid otherUserId)
    {
        return _db.Messages
            .Where(m => !m.IsDeleted &&
                        ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                         (m.SenderId == otherUserId && m.ReceiverId == userId)))
            .OrderByDescending(m => m.CreatedAt)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync();
    }
}