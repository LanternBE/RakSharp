using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class ConnectedPing : OnlineMessage {
    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.ConnectedPing;
    
    public long Time { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {
        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(Acknowledgement));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        writer.WriteLongBigEndian(Time);
    }

    protected override void ReadPayload(BinaryReader reader) {
        Time = reader.ReadLongBigEndian();
    }
}