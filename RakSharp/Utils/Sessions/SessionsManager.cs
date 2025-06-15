using System.Net;

namespace RakSharp.Utils.Sessions;

public class SessionsManager {
    
    private Dictionary<IPEndPoint, ClientSession> Sessions { get; } = new();
    
    public bool AddSession(IPEndPoint ipEndPoint, ClientSession connection) {
        return Sessions.TryAdd(ipEndPoint, connection);
    }

    public ClientSession CreateSession(IPEndPoint ipEndPoint, long clientId) {

        var session = GetSession(ipEndPoint);
        if (session != null)
            return session;
        
        var connection = new ClientSession(clientId, ipEndPoint);
        AddSession(ipEndPoint, connection);
        
        return connection;
    }
    
    public ClientSession? GetSession(IPEndPoint ipEndPoint) {
        return Sessions.GetValueOrDefault(ipEndPoint);
    }
    
    public bool RemoveSession(IPEndPoint ipEndPoint) {
        return Sessions.Remove(ipEndPoint);
    }
    
    public Dictionary<IPEndPoint, ClientSession> GetSessions() {
        return Sessions;
    }
}