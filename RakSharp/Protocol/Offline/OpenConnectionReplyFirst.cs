using System.Net;
using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Offline;

public class OpenConnectionReplyFirst : OfflineMessage {
    
    public override MessagesIdentifier.OfflineMessages PacketId => MessagesIdentifier.OfflineMessages.OpenConnectionReplyFirst;

    internal long ServerId { get; private set; }
    internal bool UseSecurity { get; private set; }
    internal short MaximumTransmissionUnit { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(OpenConnectionReplyFirst));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteMagic();
        writer.WriteLongBigEndian(ServerId);
        writer.WriteBoolean(false);
        writer.WriteShortBigEndian(MaximumTransmissionUnit);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        reader.ReadMagic();
        ServerId = reader.ReadLongBigEndian();
        UseSecurity = reader.ReadBoolean();
        MaximumTransmissionUnit = reader.ReadShortBigEndian();
    }

    public static (OpenConnectionReplyFirst packet, byte[] buffer) Create(long serverId, bool useSecurity, short maximumTransmissionUnit) {
        
        return OfflineMessage.Create<OpenConnectionReplyFirst>(packet => {
            packet.ServerId = serverId;
            packet.UseSecurity = useSecurity;
            packet.MaximumTransmissionUnit = maximumTransmissionUnit;
        });
    }
}