using System.Net;
using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class NewIncomingConnection : OnlineMessage {
    
    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.NewIncomingConnection;

    public IPEndPoint? ServerAddress { get; private set; }
    public long ClientId { get; private set; }
    public long Time { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {
        
        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(NewIncomingConnection));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteIpEndPoint(ServerAddress);
        for (var count = 0; count < 20; count++) {
            writer.WriteIpEndPoint(ServerAddress);
        }
        
        writer.WriteLongBigEndian(ClientId);
        writer.WriteLongBigEndian(Time);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        ServerAddress = reader.ReadIpEndPoint();
        for (var i = 0; i < 20; i++) {
            reader.ReadIpEndPoint();
        }

        ClientId = reader.ReadLongBigEndian();
        Time = reader.ReadLongBigEndian();
    }
    
    public static (NewIncomingConnection packet, byte[] buffer) Create(IPEndPoint serverAddress, long clientId, long time) {
        
        return OnlineMessage.Create<NewIncomingConnection>(packet => {
            packet.ServerAddress = serverAddress;
            packet.ClientId = clientId;
            packet.Time = time;
        });
    }
}