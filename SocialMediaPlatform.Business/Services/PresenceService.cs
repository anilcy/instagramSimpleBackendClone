using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using SocialMediaPlatform.Business.Interfaces;

namespace SocialMediaPlatform.Business.Services;

public class PresenceService : IPresenceService
{
    private readonly IDatabase _db;

    public PresenceService(IConnectionMultiplexer redis)
    {
        // Redis connection multiplexer should be registered as singleton in Program.cs
        _db = redis.GetDatabase();
    }

    // Active connections are stored in a Redis set with this key pattern.
    private static string ConnSet(Guid userId) => $"presence:{userId}:conn";

    // Last seen is stored as a Redis string with this key pattern (epoch seconds).
    private static string LastSeenKey(Guid userId) => $"presence:{userId}:last";

    public async Task SetOnlineAsync(Guid userId, string connectionId)
    {
        // Add it to the user's active connection set.
        await _db.SetAddAsync(ConnSet(userId), connectionId);
    }

    public async Task SetOfflineAsync(Guid userId, string connectionId)
    {
        // Delete the connection from the set.
        await _db.SetRemoveAsync(ConnSet(userId), connectionId);

        // If there are no connections left, write the last seen time.
        var left = await _db.SetLengthAsync(ConnSet(userId));
        if (left == 0)
        {
            var epoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // StringGet/Set returns RedisValue ; we will set it as string and parse it back to long when reading.
            await _db.StringSetAsync(LastSeenKey(userId), epoch.ToString());
        }
    }

    public async Task<bool> IsOnlineAsync(Guid userId)
    {
        //  If there is at least one connection in the set, consider online.
        return await _db.SetLengthAsync(ConnSet(userId)) > 0;
    }

    public async Task<DateTimeOffset?> GetLastSeenAsync(Guid userId)
    {
        var raw = await _db.StringGetAsync(LastSeenKey(userId));
        if (raw.IsNullOrEmpty) return null;

        // RedisValue -> string -> long -> DateTime
        if (!long.TryParse((string)raw, out var seconds))
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
    }
}