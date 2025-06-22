using RakSharp.Protocol.Handlers.Offline;
using RakSharp.Protocol.Handlers.Online;
using RakSharp.Protocol.Handlers.Online.EncapsulatedPackets;
using RakSharp.Protocol.Offline;
using RakSharp.Protocol.Online;

namespace RakSharp.Protocol.Handlers;

public static class HandlerSystem {
    
    private static readonly Dictionary<Type, Type> PacketHandlers = new();

    public static void InitializeDefaultHandlers() {
        
        RegisterHandler<UnconnectedPing, UnconnectedPingHandler>();
        RegisterHandler<UnconnectedPong, UnconnectedPongHandler>();
        RegisterHandler<OpenConnectionRequestFirst, OpenConnectionRequestFirstHandler>();
        RegisterHandler<OpenConnectionReplyFirst, OpenConnectionReplyFirstHandler>();
        RegisterHandler<OpenConnectionRequestSecond, OpenConnectionRequestSecondHandler>();
        RegisterHandler<OpenConnectionReplySecond, OpenConnectionReplySecondHandler>();
        RegisterHandler<Datagram, DatagramHandler>();
        RegisterHandler<Acknowledgement, AcknowledgementHandler>();
        RegisterHandler<NegativeAcknowledgement, NegativeAcknowledgementHandler>();
        
        RegisterHandler<ConnectedPing, ConnectedPingHandler>();
        RegisterHandler<ConnectionRequest, ConnectionRequestHandler>();
    }

    public static void RegisterHandler<TPacket, THandler>() where THandler : class {
        PacketHandlers[typeof(TPacket)] = typeof(THandler);
    }

    public static Type GetHandlerType(Type packetType) {
        return PacketHandlers[packetType];
    }

    public static object CreateHandler(Type handlerType) {
        return Activator.CreateInstance(handlerType);
    }
}