using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Offline;

public class UnconnectedPing : OfflineMessage {
    
    public override MessagesIdentifier.OfflineMessages PacketId => MessagesIdentifier.OfflineMessages.UnconnectedPing;

    internal long Timestamp { get; private set; }
    internal long ClientId { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(UnconnectedPing));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteLongBigEndian(Timestamp);
        writer.WriteMagic();
        writer.WriteLongBigEndian(ClientId);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        Timestamp = reader.ReadLongBigEndian();
        reader.ReadMagic();
        ClientId = reader.ReadLongBigEndian();
    }

    public static (UnconnectedPing packet, byte[] buffer) Create(long timestamp, long clientId) {
        
        return OfflineMessage.Create<UnconnectedPing>(packet => {
            packet.Timestamp = timestamp;
            packet.ClientId = clientId;
        });
    }
}