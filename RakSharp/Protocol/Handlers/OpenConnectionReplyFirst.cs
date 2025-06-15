using RakSharp.Protocol.Offline;

namespace RakSharp.Protocol.Handlers;

public class OpenConnectionReplyFirstHandler : OfflinePacketHandler<OpenConnectionReplyFirst> {
    
    public override async Task<bool> HandleAsync() {

        var response = OpenConnectionRequestSecond.Create(Server.ServerAddress, 1492, 0); // TODO: Idk what i did here ngl.
        await SendOfflineMessageAsync(response);
        
        return true;
    }
}