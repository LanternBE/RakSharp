using System.Net;
using System.Net.Sockets;
using RakSharp.Packet;
using RakSharp.Protocol;
using RakSharp.Protocol.Handlers;
using RakSharp.Utils;
using RakSharp.Utils.Sessions;

namespace RakSharp;

public class Server {

    public IPEndPoint ServerAddress { get; set; } = new(IPAddress.Any, 19132);
    public Socket Socket { get; set; } = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    public ServerInfo ServerInfo { get; set; } = new();
    public SessionsManager SessionsManager { get; set; } = new();
    public PacketProcessor PacketProcessor { get; set; }

    public bool IsRunning { get; set; }

    public async Task Start() {

        if (IsRunning) {
            Logger.LogWarn("RakNet is already running.");
            return;
        }

        Logger.LogInfo("RakNet started.");
        var buffer = new byte[10000]; // Not sure about this value tbh.

        Socket.EnableBroadcast = true;
        Socket.Bind(ServerAddress);
        
        HandlerSystem.InitializeDefaultHandlers();
        IsRunning = true;
        
        PacketProcessor = new PacketProcessor(Socket, this);
        while (IsRunning) {
            
            var sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteEndPoint = sender;
            
            var received = await Socket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEndPoint);
            var packet = DynamicPacketFactory.CreatePacketFromBufferAuto(buffer);
            if (packet is null) {
                Logger.LogWarn($"Failed to parse packet from {remoteEndPoint}");
                continue;
            }
            
            var clientIpEndPoint = (IPEndPoint)received.RemoteEndPoint;
            var success = await PacketProcessor.ProcessPacketAsync(packet, buffer, clientIpEndPoint);

            if (!success)
                Logger.LogError($"Failed to parse packet from {clientIpEndPoint}");
        }
    }

    public async Task Stop() {
        
        IsRunning = false;
        Logger.LogInfo("RakNet stopped.");
        
        await Task.Delay(0); // TODO: Need to remove this, i used it just to disable the warning from the ide lol.
    }
}
