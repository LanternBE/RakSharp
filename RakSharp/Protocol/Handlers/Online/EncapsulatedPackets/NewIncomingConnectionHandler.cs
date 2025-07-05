using RakSharp.Protocol.Online;
using RakSharp.Utils;
using RakSharp.Utils.Sessions;

namespace RakSharp.Protocol.Handlers.Online.EncapsulatedPackets;

public class NewIncomingConnectionHandler : EncapsulatedPacketHandler<NewIncomingConnection> {
    
    public override async Task<bool> HandleAsync() {

        var clientSession = Server.SessionsManager.GetSession(ClientEndPoint);
        if (clientSession is null) {
            return false;
        }
        
        Logger.LogDebug($"Received NewIncomingConnection from ({ClientEndPoint})");
        if (Packet.ServerAddress is null) {
            
            Logger.LogWarn($"NewIncomingConnection ServerAddress not found, ClientSession ({clientSession.RemoteEndPoint}).");
            await DisconnectClientSession(clientSession);
            
            return false;
        }
        
        if (Packet.ServerAddress.Port != Server.ServerAddress.Port) {
            
            Logger.LogWarn($"ClientSession ({clientSession.RemoteEndPoint}) tried to join on a different endpoint ({Packet.ServerAddress}), actual endpoint ({Server.ServerAddress}).");
            await DisconnectClientSession(clientSession);
            
            return false;
        }
        
        await SendConnectedPongAsync(Packet.Time);
        return true;
    }

    private async Task DisconnectClientSession(ClientSession clientSession) {
        
        var disconnect = Disconnect.Create();
        var response = EncapsulatedPacket.Create(disconnect.packet, PacketReliability.Reliable, clientSession.GetNextReliableIndex(), clientSession.GetNextOrderedIndex());

        await SendEncapsulatedPacketAsync((response, response.Buffer));
        clientSession.Disconnect();
    }
}