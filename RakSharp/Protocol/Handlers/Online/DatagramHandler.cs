using System.Net;
using RakSharp.Packet;
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
        clientSession.UpdateLastPacketTime();
        
        foreach (var encapsulatedPacket in Packet.Packets) {

            var packet = EncapsulatedPacketFactory.CreateFromBuffer(encapsulatedPacket.Buffer);
            if (packet is null) {
                Logger.LogError("Error while parsing an EncapsulatedPacket");
                continue;
            }
            
            var success = await Server.PacketProcessor.ProcessPacketAsync(packet, encapsulatedPacket.Buffer, ClientEndPoint);
            if (!success) {
                Logger.LogError("Error while handling an EncapsulatedPacket");
            }
        }
        
        return true;
    }
    
    private bool IsBannedIp(IPAddress _) {
        
        // TODO: Implement banned IPs.
        return false;
    }
}