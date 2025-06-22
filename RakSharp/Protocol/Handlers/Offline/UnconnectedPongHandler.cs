using RakSharp.Protocol.Offline;

namespace RakSharp.Protocol.Handlers.Offline;

public class UnconnectedPongHandler : OfflinePacketHandler<UnconnectedPong> {
    
    public override async Task<bool> HandleAsync() {
        
        var response = OpenConnectionRequestFirst.Create(MessagesIdentifier.RakNetVersion, 1492);
        await SendOfflineMessageAsync(response);
        
        return true;
    }
}