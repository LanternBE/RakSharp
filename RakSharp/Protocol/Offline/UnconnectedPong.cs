using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Offline;

public class UnconnectedPong : OfflineMessage {
    
    public override MessagesIdentifier.OfflineMessages PacketId => MessagesIdentifier.OfflineMessages.UnconnectedPong;

    internal long Timestamp { get; private set; }
    internal long ServerId { get; private set; }
    internal string ServerInfo { get; private set; } = string.Empty;
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(UnconnectedPong));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteLongBigEndian(Timestamp);
        writer.WriteLongBigEndian(ServerId);
        writer.WriteMagic();
        writer.WriteString(ServerInfo);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        Timestamp = reader.ReadLongBigEndian();
        ServerId = reader.ReadLongBigEndian();
        reader.ReadMagic();
        ServerInfo = reader.ReadString();
    }

    public static (UnconnectedPong packet, byte[] buffer) Create(long timestamp, long serverId, string serverInfo) {
        
        return OfflineMessage.Create<UnconnectedPong>(packet => {
            packet.Timestamp = timestamp;
            packet.ServerId = serverId;
            packet.ServerInfo = serverInfo;
        });
    }
}