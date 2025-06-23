using System.Net;
using System.Net.Sockets;
using RakSharp.Protocol.Handlers;

namespace RakSharp.Protocol;

public class PacketProcessor(Socket socket, Server server, HandlerSystem handlerSystem) {
    
    public async Task<bool> ProcessPacketAsync(object packet, byte[] buffer, IPEndPoint clientEndPoint) {
        
        var packetType = packet.GetType();
        var handlerType = handlerSystem.GetHandlerType(packetType);

        if (handlerType is null)
            return false;

        var handler = handlerSystem.CreateHandler(handlerType);
        var initializeMethod = handlerType.GetMethod("Initialize");

        if (initializeMethod == null)
            return false;

        initializeMethod.Invoke(handler, [server, socket, clientEndPoint, buffer, packet]);
        var handleMethod = handlerType.GetMethod("HandleAsync");

        if (handleMethod == null)
            return false;

        var result = handleMethod.Invoke(handler, null);
        if (result is Task<bool> task) {
            return await task;
        }

        return false;
    }
}