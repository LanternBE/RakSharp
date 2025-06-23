using System.Net;
using RakSharp.Protocol.Offline;
using RakSharp.Utils;

namespace RakSharp.Protocol.Handlers.Offline;

public class UnconnectedPingHandler : OfflinePacketHandler<UnconnectedPing> {
    
    public override async Task<bool> HandleAsync() {
        
        if (IsBannedIp(ClientEndPoint.Address)) {
            Logger.LogWarn($"Banned IP {ClientEndPoint} attempted to ping");
            return false;
        }

        Server.ServerInfo.OnlinePlayers = Server.SessionsManager.GetSessions().Count;
        var response = UnconnectedPong.Create(
            Packet.Timestamp, 
            Server.ServerInfo.ServerGuid, 
            Server.ServerInfo.ToString()
        );
        
        await SendOfflineMessageAsync(response);
        return true;
    }
    
    private bool IsBannedIp(IPAddress _) {
        
        // TODO: Implement banned IPs.
        return false;
    }
}