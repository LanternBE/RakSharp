using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Offline;

public class IncompatibleProtocolVersion : OfflineMessage {
    
    public override MessagesIdentifier.OfflineMessages PacketId => MessagesIdentifier.OfflineMessages.IncompatibleProtocolVersion;

    internal byte RakNetVersion { get; private set; }
    internal long ServerId { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(IncompatibleProtocolVersion));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteByte(RakNetVersion);
        writer.WriteMagic();
        writer.WriteLongBigEndian(ServerId);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        RakNetVersion = reader.ReadByte();
        reader.ReadMagic();
        ServerId = reader.ReadLongBigEndian();
    }

    public static (IncompatibleProtocolVersion packet, byte[] buffer) Create(byte rakNetVersion, long serverId) {
        
        return OfflineMessage.Create<IncompatibleProtocolVersion>(packet => {
            packet.RakNetVersion = rakNetVersion;
            packet.ServerId = serverId;
        });
    }
}