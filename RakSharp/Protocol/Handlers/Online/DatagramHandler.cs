using System.Net;
using RakSharp.Protocol.Online;
using RakSharp.Utils;

namespace RakSharp.Protocol.Handlers.Online;

public class DatagramHandler : OnlinePacketHandler<Datagram> {
    
    public override async Task<bool> HandleAsync() {
        
        if (IsBannedIp(ClientEndPoint.Address)) {
            Logger.LogWarn($"Banned IP {ClientEndPoint} attempted to ping");
            return false;
        }
        
        var clientSession = Server.SessionsManager.GetSession(ClientEndPoint);
        if (clientSession is null) {
            Logger.LogError($"No Session found for ({ClientEndPoint}) while handling his datagram.");
            return false;
        }

        await SendOnlineMessageAsync(Acknowledgement.Create([Packet.SeqNumber]));
        Logger.LogInfo($"Sent acknowledgement to server {ClientEndPoint}");
        
        clientSession.UpdateLastPacketTime();
        // TODO: Create a function that can handle all the Packet.Packets, maybe creating a new handler for these packets...
        
        return true;
    }
    
    private bool IsBannedIp(IPAddress _) {
        
        // TODO: Implement banned IPs.
        return false;
    }
}