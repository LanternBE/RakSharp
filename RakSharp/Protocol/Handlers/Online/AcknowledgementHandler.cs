using System.Net;
using RakSharp.Protocol.Online;
using RakSharp.Utils;

namespace RakSharp.Protocol.Handlers.Online;

public class AcknowledgementHandler : OnlinePacketHandler<Acknowledgement> {
    
    public override async Task<bool> HandleAsync() {
        
        if (IsBannedIp(ClientEndPoint.Address)) {
            Logger.LogWarn($"Banned IP {ClientEndPoint} attempted to ping");
            return false;
        }

        var clientSession = Server.SessionsManager.GetSession(ClientEndPoint);
        if (clientSession is null) {
            Logger.LogDebug($"Ignored a Acknowledgement from a client ({ClientEndPoint}) without session.");
            return false;
        }
        
        clientSession.HandleAcknowledgement(Packet);
        return true;
    }
    
    private bool IsBannedIp(IPAddress _) {
        
        // TODO: Implement banned IPs.
        return false;
    }
}