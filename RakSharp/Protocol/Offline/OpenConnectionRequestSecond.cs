using System.Net;
using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Offline;

public class OpenConnectionRequestSecond : OfflineMessage {
    
    public override MessagesIdentifier.OfflineMessages PacketId => MessagesIdentifier.OfflineMessages.OpenConnectionRequestSecond;

    internal IPEndPoint? ServerAddress { get; private set; }
    internal short MaximumTransmissionUnit { get; private set; }
    internal long ClientId { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(OpenConnectionRequestSecond));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteMagic();
        writer.WriteIPEndPoint(ServerAddress);
        writer.WriteShortBigEndian(MaximumTransmissionUnit);
        writer.WriteLongBigEndian(ClientId);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        reader.ReadMagic();
        ServerAddress = reader.ReadIPEndPoint();
        MaximumTransmissionUnit = reader.ReadShortBigEndian();
        ClientId = reader.ReadLongBigEndian();
    }

    public static (OpenConnectionRequestSecond packet, byte[] buffer) Create(IPEndPoint? serverAddress, short maximumTransmissionUnit, long clientId) {
        
        return OfflineMessage.Create<OpenConnectionRequestSecond>(packet => {
            packet.ServerAddress = serverAddress;
            packet.MaximumTransmissionUnit = maximumTransmissionUnit;
            packet.ClientId = clientId;
        });
    }
}