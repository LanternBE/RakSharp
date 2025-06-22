using RakSharp.Protocol.Online;

namespace RakSharp.Protocol.Handlers.EncapsulatedPackets;

public class ConnectedPingHandler : EncapsulatedPacketHandler<ConnectedPing> {
    
    public override async Task<bool> HandleAsync() {

        await SendConnectedPongAsync(Packet.Time);
        return true;
    }
}