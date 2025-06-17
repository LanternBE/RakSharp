using RakSharp.Protocol.Offline;

namespace RakSharp.Protocol.Handlers;

public class OpenConnectionReplySecondHandler : OfflinePacketHandler<OpenConnectionReplySecond> {
    
    public override async Task<bool> HandleAsync() {
        // TODO: Need to create a datagram and sending it as OnlineMessage.
        return true;
    }
}