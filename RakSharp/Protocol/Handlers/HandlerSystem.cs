using RakSharp.Protocol.Handlers.Offline;
using RakSharp.Protocol.Handlers.Online;
using RakSharp.Protocol.Handlers.Online.EncapsulatedPackets;
using RakSharp.Protocol.Offline;
using RakSharp.Protocol.Online;

namespace RakSharp.Protocol.Handlers;

public class HandlerSystem {
    
    private readonly Dictionary<Type, Type> _packetHandlers = new();

    public void InitializeDefaultHandlers() {
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
        RegisterHandler<NewIncomingConnection, NewIncomingConnectionHandler>();
        RegisterHandler<Disconnect, DisconnectHandler>();
    }

    public void RegisterHandler<TPacket, THandler>() where THandler : class {
        _packetHandlers[typeof(TPacket)] = typeof(THandler);
    }

    public Type? GetHandlerType(Type packetType) {
        return _packetHandlers.TryGetValue(packetType, out var handlerType) ? handlerType : null;
    }

    public object CreateHandler(Type handlerType) {
        return Activator.CreateInstance(handlerType);
    }
}