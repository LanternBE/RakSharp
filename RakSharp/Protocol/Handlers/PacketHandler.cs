using System.Net;
using System.Net.Sockets;
using RakSharp.Packet;

namespace RakSharp.Protocol.Handlers;

public abstract class OfflinePacketHandler<T> {
    
    protected Server Server { get; set; }
    protected Socket Socket { get; set; }
    protected IPEndPoint ClientEndPoint { get; set; }
    protected byte[] Buffer { get; set; }
    protected T Packet { get; set; }

    public void Initialize(Server server, Socket socket, IPEndPoint clientEndPoint, byte[] buffer, T packet) {
        
        Server = server;
        Socket = socket;
        ClientEndPoint = clientEndPoint;
        Buffer = buffer;
        Packet = packet;
    }

    public abstract Task<bool> HandleAsync();

    protected async Task SendOfflineMessageAsync((OfflineMessage packet, byte[] buffer) response) {
        await Socket.SendToAsync(response.buffer, SocketFlags.None, ClientEndPoint);
    }
}

public abstract class OnlinePacketHandler<T> {
    
    protected Server Server { get; set; }
    protected Socket Socket { get; set; }
    protected IPEndPoint ClientEndPoint { get; set; }
    protected byte[] Buffer { get; set; }
    protected T Packet { get; set; }

    public void Initialize(Server server, Socket socket, IPEndPoint clientEndPoint, byte[] buffer, T packet) {
        
        Server = server;
        Socket = socket;
        ClientEndPoint = clientEndPoint;
        Buffer = buffer;
        Packet = packet;
    }

    public abstract Task<bool> HandleAsync();

    protected async Task SendOnlineMessageAsync((OnlineMessage packet, byte[] buffer) response) {
        await Socket.SendToAsync(response.buffer, SocketFlags.None, ClientEndPoint);
    }
}