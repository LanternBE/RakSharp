using RakSharp.Protocol.Online;

namespace RakSharp.Protocol.Handlers.Online;

public class ConnectedPingHandler : OnlinePacketHandler<ConnectedPing> {
    
    public override Task<bool> HandleAsync() {
        throw new NotImplementedException();
    }
}