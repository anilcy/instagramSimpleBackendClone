// using System.Collections.Concurrent;
// using instagramClone.Business.Interfaces;

using System.Collections.Concurrent;
using instagramClone.Business.Interfaces;

public class InMemoryPresenceService : IPresenceService
{
    
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();
    private static readonly ConcurrentDictionary<Guid, DateTime> _lastSeen = new();

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
                    _lastSeen[userId] = DateTime.UtcNow;
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

    public Task<DateTime?> GetLastSeenAsync(Guid userId)
    {
        if (_lastSeen.TryGetValue(userId, out var t)) return Task.FromResult<DateTime?>(t);
        return Task.FromResult<DateTime?>(null);
    }
}