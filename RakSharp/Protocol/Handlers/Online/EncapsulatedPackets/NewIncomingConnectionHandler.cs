using RakSharp.Protocol.Online;
using RakSharp.Utils;

namespace RakSharp.Protocol.Handlers.Online.EncapsulatedPackets;

public class NewIncomingConnectionHandler : EncapsulatedPacketHandler<NewIncomingConnection> {
    
    public override async Task<bool> HandleAsync() {

        var clientSession = Server.SessionsManager.GetSession(ClientEndPoint);
        if (clientSession is null) {
            return false;
        }
        
        Logger.LogDebug($"Received NewIncomingConnection from ({ClientEndPoint})");
        await SendConnectedPongAsync(Packet.Time);

        return true;
    }
}