using System.Collections.Concurrent;
using Application;

namespace Api.Hub;

public class UserConnectionManager : IUserConnectionManager
{
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> UserConnections = new();

    public void AddConnection(Guid userId, string connectionId)
    {
        Verify.NotNullOrWhiteSpace(connectionId);
        
        UserConnections.AddOrUpdate(userId, 
            _ => new HashSet<string> { connectionId }, 
            (_, connections) => { connections.Add(connectionId); return connections; });
    }

    public void RemoveConnection(string connectionId)
    {
        Verify.NotNullOrWhiteSpace(connectionId);
        
        foreach (var pair in UserConnections.ToList())
        {
            if (!pair.Value.Contains(connectionId)) continue;

            pair.Value.Remove(connectionId);

            if (pair.Value.Count == 0)
            {
                UserConnections.TryRemove(pair.Key, out _);
            }

            break;
        }
    }

    public List<string> GetConnections(Guid userId)
    {
        return UserConnections.TryGetValue(userId, out var connections) ? connections.ToList() : new List<string>();
    }
}

public interface IUserConnectionManager
{
    void AddConnection(Guid userId, string connectionId);
    void RemoveConnection(string connectionId);
    List<string> GetConnections(Guid userId);
}