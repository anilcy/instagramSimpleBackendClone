// using System.Collections.Concurrent;
// using SocialMediaPlatform.Business.Interfaces;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialMediaPlatform.Business.Interfaces;

public class InMemoryPresenceService : IPresenceService
{
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();
    private static readonly ConcurrentDictionary<Guid, DateTimeOffset> _lastSeen = new();

    public Task SetOnlineAsync(Guid userId, string connectionId)
    {
        var set = _connections.GetOrAdd(userId, _ => new HashSet<string>());
        lock (set) set.Add(connectionId);
        return Task.CompletedTask;
    }

    public Task SetOfflineAsync(Guid userId, string connectionId)
    {
        if (_connections.TryGetValue(userId, out var set))
        {
            lock (set)
            {
                set.Remove(connectionId);
                if (set.Count == 0)
                {
                    _connections.TryRemove(userId, out _);
                    _lastSeen[userId] = DateTimeOffset.UtcNow;
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsOnlineAsync(Guid userId)
    {
        var online = _connections.ContainsKey(userId);
        return Task.FromResult(online);
    }

    public Task<DateTimeOffset?> GetLastSeenAsync(Guid userId)
    {
        if (_lastSeen.TryGetValue(userId, out var t))
            return Task.FromResult<DateTimeOffset?>(t);
        return Task.FromResult<DateTimeOffset?>(null);
    }
}