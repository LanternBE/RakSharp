using RakSharp.Protocol.Online;
using RakSharp.Utils;

namespace RakSharp.Protocol.Handlers.Online.EncapsulatedPackets;

public class ConnectionRequestHandler : EncapsulatedPacketHandler<ConnectionRequest> {
    
    public override async Task<bool> HandleAsync() {

        var clientSession = Server.SessionsManager.GetSession(ClientEndPoint);
        if (clientSession is null) {
            return false;
        }
        
        Logger.LogDebug($"Received ConnectionRequest from ({ClientEndPoint})");
        
        var connectionRequestAccepted = ConnectionRequestAccepted.Create(ClientEndPoint, [], DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 0);
        var response = EncapsulatedPacket.Create(connectionRequestAccepted.packet, PacketReliability.Reliable, clientSession.GetNextReliableIndex(), clientSession.GetNextOrderedIndex());

        await SendEncapsulatedPacketAsync((response, response.Buffer));
        return true;
    }
}