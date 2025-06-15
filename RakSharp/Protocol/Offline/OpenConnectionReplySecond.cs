using System.Net;
using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Offline;

public class OpenConnectionReplySecond : OfflineMessage {
    
    public override MessagesIdentifier.OfflineMessages PacketId => MessagesIdentifier.OfflineMessages.OpenConnectionReplySecond;

    internal long ServerId { get; private set; }
    internal IPEndPoint? ClientAddress { get; private set; }
    internal short MaximumTransmissionUnit { get; private set; }
    internal bool HasEncryption { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(OpenConnectionReplySecond));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteMagic();
        writer.WriteLongBigEndian(ServerId);
        writer.WriteIpEndPoint(ClientAddress);
        writer.WriteShortBigEndian(MaximumTransmissionUnit);
        writer.WriteBoolean(HasEncryption);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        reader.ReadMagic();
        ServerId = reader.ReadLongBigEndian();
        ClientAddress = reader.ReadIpEndPoint();
        MaximumTransmissionUnit = reader.ReadShortBigEndian();
        HasEncryption = reader.ReadBoolean();
    }

    public static (OpenConnectionReplySecond packet, byte[] buffer) Create(long serverId, IPEndPoint clientAddress, short maximumTransmissionUnit, bool hasEncryption) {
        
        return OfflineMessage.Create<OpenConnectionReplySecond>(packet => {
            packet.ServerId = serverId;
            packet.ClientAddress = clientAddress;
            packet.MaximumTransmissionUnit = maximumTransmissionUnit;
            packet.HasEncryption = hasEncryption;
        });
    }
}