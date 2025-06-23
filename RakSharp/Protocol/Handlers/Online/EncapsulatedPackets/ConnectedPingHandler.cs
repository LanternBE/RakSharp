using RakSharp.Protocol.Online;

namespace RakSharp.Protocol.Handlers.Online.EncapsulatedPackets;

public class ConnectedPingHandler : EncapsulatedPacketHandler<ConnectedPing> {
    
    public override async Task<bool> HandleAsync() {

        await SendConnectedPongAsync(Packet.Time);
        return true;
    }
}