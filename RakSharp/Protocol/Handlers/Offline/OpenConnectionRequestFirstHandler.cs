using RakSharp.Protocol.Offline;

namespace RakSharp.Protocol.Handlers;

public class OpenConnectionRequestFirstHandler : OfflinePacketHandler<OpenConnectionRequestFirst> {
    
    public override async Task<bool> HandleAsync() {
        
        var response = OpenConnectionReplyFirst.Create(Server.ServerInfo.ServerGuid, false, 1492); // TODO: Add encryption.
        await SendOfflineMessageAsync(response);
        
        return true;
    }
}