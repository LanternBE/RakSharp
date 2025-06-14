using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Offline;

public class OpenConnectionRequestFirst : OfflineMessage {
    
    public override MessagesIdentifier.OfflineMessages PacketId => MessagesIdentifier.OfflineMessages.OpenConnectionRequestFirst;

    internal byte ProtocolVersion { get; private set; }
    internal short MaximumTransmissionUnit { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(OpenConnectionRequestFirst));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteMagic();
        writer.WriteByte(ProtocolVersion);
        writer.WriteShortLittleEndian(MaximumTransmissionUnit);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        reader.ReadMagic();
        ProtocolVersion = reader.ReadByte();
        MaximumTransmissionUnit = reader.ReadShortLittleEndian();
    }

    public static (OpenConnectionRequestFirst packet, byte[] buffer) Create(byte protocolVersion, short maximumTransmissionUnit) {
        
        return OfflineMessage.Create<OpenConnectionRequestFirst>(packet => {
            packet.ProtocolVersion = protocolVersion;
            packet.MaximumTransmissionUnit = maximumTransmissionUnit;
        });
    }
}