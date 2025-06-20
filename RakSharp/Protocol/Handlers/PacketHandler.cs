using System.Net;
using System.Net.Sockets;
using RakSharp.Packet;
using RakSharp.Protocol.Online;
using RakSharp.Utils;

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

public abstract class EncapsulatedPacketHandler<T> {
    
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

    protected async Task SendEncapsulatedPacketAsync((EncapsulatedPacket packet, byte[] buffer) response) {
        var session = Server.SessionsManager.GetSession(ClientEndPoint);
        if (session is null) {
            Logger.LogError($"Session not found for the client endpoint ({ClientEndPoint})");
            return;
        }
        var datagram = Datagram.Create(0, [response.packet], session.GetNextSequenceNumber());
        
        await Socket.SendToAsync(datagram.buffer, SocketFlags.None, ClientEndPoint);
    }

    protected async Task SendConnectedPongAsync(long time) {
        var session = Server.SessionsManager.GetSession(ClientEndPoint);
        
        if (session == null) {
            Logger.LogWarn($"Received connected ping from unknown session: {ClientEndPoint}");
            return;
        }
        
        var connectedPong = EncapsulatedPacket.Create(ConnectedPong.Create(time, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()), PacketReliability.Reliable, session.GetNextReliableIndex(), session.GetNextOrderedIndex());
        if (connectedPong is null) {
            Logger.LogError($"Failed to create connected pong for session: {ClientEndPoint}");
            return;
        }
        if (connectedPong.Reliability is PacketReliability.Reliable or PacketReliability.ReliableOrdered) {
            session.TrackReliablePacket(session.GetNextSequenceNumber());
        }
    }
}