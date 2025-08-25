using StackExchange.Redis;
using instagramClone.Business.Interfaces;

namespace instagramClone.Business.Services;

public class PresenceService : IPresenceService
{
    private readonly IDatabase _db;

    public PresenceService(IConnectionMultiplexer redis)
    {
        // Redis connection multiplexer Program.cs içinde singleton olarak register edilmeli
        _db = redis.GetDatabase();
    }

    // Aktif bağlantıların tutulduğu set anahtarı
    private static string ConnSet(Guid userId) => $"presence:{userId}:conn";

    // Son görülmenin tutulduğu key (epoch saniye olarak)
    private static string LastSeenKey(Guid userId) => $"presence:{userId}:last";

    public async Task SetOnlineAsync(Guid userId, string connectionId)
    {
        // Kullanıcının aktif bağlantı setine ekle
        await _db.SetAddAsync(ConnSet(userId), connectionId);
    }

    public async Task SetOfflineAsync(Guid userId, string connectionId)
    {
        // Bağlantıyı sil
        await _db.SetRemoveAsync(ConnSet(userId), connectionId);

        // Eğer hiç bağlantı kalmadıysa son görülmeyi yaz
        var left = await _db.SetLengthAsync(ConnSet(userId));
        if (left == 0)
        {
            var epoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // StringGet/Set RedisValue döner; string olarak set ediyoruz
            await _db.StringSetAsync(LastSeenKey(userId), epoch.ToString());
        }
    }

    public async Task<bool> IsOnlineAsync(Guid userId)
    {
        // Set’te en az bir connection varsa online kabul
        return await _db.SetLengthAsync(ConnSet(userId)) > 0;
    }

    public async Task<DateTime?> GetLastSeenAsync(Guid userId)
    {
        var raw = await _db.StringGetAsync(LastSeenKey(userId));
        if (raw.IsNullOrEmpty) return null;

        // RedisValue -> string -> long -> DateTime
        if (!long.TryParse((string)raw, out var seconds))
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
    }
}