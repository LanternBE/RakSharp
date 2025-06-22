using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class ConnectedPong : OnlineMessage {

    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.ConnectedPong;
    
    public long Ping { get; private set; }
    public long Pong { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {
        
        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(ConnectedPong));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteLongBigEndian(Ping);
        writer.WriteLongBigEndian(Pong);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        Ping = reader.ReadLongBigEndian();
        Pong = reader.ReadLongBigEndian();
    }

    public static (ConnectedPong packet, byte[] buffer) Create(long ping, long pong) {
        
        return OnlineMessage.Create<ConnectedPong>(packet => {
            packet.Ping = ping;
            packet.Pong = pong;
        });
    }
}