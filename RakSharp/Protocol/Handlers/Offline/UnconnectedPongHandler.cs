using System.Net;
using RakSharp.Protocol.Offline;
using RakSharp.Utils;

namespace RakSharp.Protocol.Handlers;

public class UnconnectedPongHandler : OfflinePacketHandler<UnconnectedPong> {
    
    public override async Task<bool> HandleAsync() {
        
        var response = OpenConnectionRequestFirst.Create(11, 1492);
        await SendOfflineMessageAsync(response);
        
        return true;
    }
}