using System.Net;
using System.Net.Sockets;
using RakSharp.Packet;
using RakSharp.Protocol;
using RakSharp.Protocol.Offline;

var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

socket.EnableBroadcast = true;
socket.Bind(new IPEndPoint(IPAddress.Any, 19132));

var serverId = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);

while (true) {

    var buffer = new byte[1492];
    var sender = new IPEndPoint(IPAddress.Any, 0);
    
    EndPoint remoteEp = sender;
    var received = await socket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEp);

    var packet = PacketFactory.CreateOfflineMessageFromBuffer(buffer);
    switch (packet) {
        
        case UnconnectedPing unconnectedPing:
            await SendOfflineMessageAsync(UnconnectedPong.Create(unconnectedPing.Timestamp, serverId, new ServerInfo {
                ServerGuid = serverId
            }.ToString()), received.RemoteEndPoint);
            break;
        
        case OpenConnectionRequestFirst openConnectionRequestFirst:

            if (openConnectionRequestFirst.ProtocolVersion != MessagesIdentifier.RakNetVersion) {
                await SendOfflineMessageAsync(IncompatibleProtocolVersion.Create(MessagesIdentifier.RakNetVersion, serverId), received.RemoteEndPoint);
            }

            await SendOfflineMessageAsync(OpenConnectionReplyFirst.Create(serverId, false, 1492), received.RemoteEndPoint);
            break;
        
        case OpenConnectionRequestSecond openConnectionRequestSecond:
            
            await SendOfflineMessageAsync(OpenConnectionReplySecond.Create(serverId, (IPEndPoint)received.RemoteEndPoint, openConnectionRequestSecond.MaximumTransmissionUnit, false), received.RemoteEndPoint);
            break;
    }
}

async Task HandleOnlineMessageAsync(OnlineMessage packet, IPEndPoint remoteEndPoint) {
    
}

async Task SendOfflineMessageAsync((OfflineMessage packet, byte[] buffer) offlineMessage, EndPoint receiver) {
    await socket.SendToAsync(offlineMessage.buffer, SocketFlags.None, receiver);
}