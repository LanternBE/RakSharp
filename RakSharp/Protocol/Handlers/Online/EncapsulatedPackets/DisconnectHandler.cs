using RakSharp.Protocol.Online;

namespace RakSharp.Protocol.Handlers.Online.EncapsulatedPackets;

public class DisconnectHandler : EncapsulatedPacketHandler<Disconnect> {
    
    public override async Task<bool> HandleAsync() {

        var clientSession = Server.SessionsManager.GetSession(ClientEndPoint);
        if (clientSession is null) {
            return false;
        }
        
        clientSession.Disconnect();
        Server.SessionsManager.RemoveSession(ClientEndPoint);
        
        return true;
    }
}