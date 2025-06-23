# RakSharp

> [!NOTE]
> This RakNet was created primarily to help develop [Lantern](https://github.com/LanternBE/Lantern), feel free to find bugs or improvements!

## How To Use

```csharp
using System.Net;
using RakSharp;

var address = new IPEndPoint(IPAddress.Any, 19132);
var server = new Server(address);

await server.Start();
```

Wait... you don't like how we handle the packets? Good! Then you can write your own handler.

```csharp
using System.Net;
using RakSharp;
using RakSharp.Protocol.Offline;

var address = new IPEndPoint(IPAddress.Any, 19132);
var server = new Server(address);

server.HandlerSystem.RegisterHandler<UnconnectedPing, MyCustomUnconnectedPingHandler>();

await server.Start();
```

```csharp
// Example Code: MyCustomUnconnectedPingHandler

using System.Net;
using RakSharp.Protocol.Offline;
using RakSharp.Utils;

namespace YourProject;

public class MyCustomUnconnectedPingHandler : OfflinePacketHandler<UnconnectedPing> {
    
    public override async Task<bool> HandleAsync() {

        Server.ServerInfo.OnlinePlayers = Server.SessionsManager.GetSessions().Count; // Maybe instead of this, you can do your own things, like:

        Server.ServerInfo.OnlinePlayers = 69 // Funny number, or you can do this:

        Server.ServerInfo.OnlinePlayers = Random.Shared.Next(0, 100); // Wow, a random players counter!

        var response = UnconnectedPong.Create(
            Packet.Timestamp, 
            Server.ServerInfo.ServerGuid, 
            Server.ServerInfo.ToString()
        );
        
        await SendOfflineMessageAsync(response);
        return true;
    }
}
```

### This was just an example of how you could use our RakNet, it has a lot of potential :)