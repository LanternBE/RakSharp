using RakSharp.Protocol.Offline;

namespace RakSharp.Protocol.Handlers;

public class OpenConnectionRequestSecondHandler : OfflinePacketHandler<OpenConnectionRequestSecond> {
    
    public override async Task<bool> HandleAsync() {
        
        var response = OpenConnectionReplySecond.Create(Server.ServerInfo.ServerGuid, Server.ServerAddress, 1492, false); // TODO: Add encryption.
        await SendOfflineMessageAsync(response);
        
        return true;
    }
}