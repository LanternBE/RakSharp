using System.Net;
using System.Net.Sockets;
using RakSharp.Protocol.Handlers;

namespace RakSharp.Protocol;

public class PacketProcessor(Socket socket, Server server) {
    
    public async Task<bool> ProcessPacketAsync(object packet, byte[] buffer, IPEndPoint clientEndPoint) {
        
        /*try {*/
            var packetType = packet.GetType();
            var handlerType = HandlerSystem.GetHandlerType(packetType);
            
            if (handlerType is null)
                return false;

            var handler = HandlerSystem.CreateHandler(handlerType);
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
        /*} catch (Exception ex) {
            Logger.LogError("Error processing packet", ex);
            return false;
        }*/
    }
}