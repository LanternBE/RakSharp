using RakSharp.Protocol.Offline;
using RakSharp.Utils;

namespace RakSharp.Protocol.Handlers.Offline;

public class OpenConnectionRequestFirstHandler : OfflinePacketHandler<OpenConnectionRequestFirst> {
    
    public override async Task<bool> HandleAsync() {

        if (Packet.ProtocolVersion != MessagesIdentifier.RakNetVersion) {
            
            await SendOfflineMessageAsync(IncompatibleProtocolVersion.Create(MessagesIdentifier.RakNetVersion, Server.ServerInfo.ServerGuid));
            Logger.LogInfo($"A client ({ClientEndPoint}) tried to join with an older version of the game, with RakNet version: {Packet.ProtocolVersion}, excepted ({MessagesIdentifier.RakNetVersion}).");

            return false;
        }
        
        var response = OpenConnectionReplyFirst.Create(Server.ServerInfo.ServerGuid, false, 1492); // TODO: Add encryption.
        await SendOfflineMessageAsync(response);
        
        return true;
    }
}