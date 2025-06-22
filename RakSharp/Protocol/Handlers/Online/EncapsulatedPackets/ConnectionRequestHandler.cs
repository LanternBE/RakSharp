using RakSharp.Protocol.Online;

namespace RakSharp.Protocol.Handlers.Online.EncapsulatedPackets;

public class ConnectionRequestHandler : EncapsulatedPacketHandler<ConnectionRequest> {
    
    public override async Task<bool> HandleAsync() {
        
        Console.WriteLine("test");
        return true;
    }
}