using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class ConnectionRequest : OnlineMessage {

    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.ConnectionRequest;
    
    public long ClientId { get; private set; }
    public long Time { get; private set; }
    public bool UseSecurity { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {
        
        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(ConnectionRequest));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteLongBigEndian(ClientId);
        writer.WriteLongBigEndian(Time);
        writer.WriteBoolean(UseSecurity);
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        ClientId = reader.ReadLongBigEndian();
        Time = reader.ReadLongBigEndian();
        UseSecurity = reader.ReadBoolean();
    }
    
    public static (ConnectionRequest packet, byte[] buffer) Create(long clientId, long time, bool useSecurity) {
        
        return OnlineMessage.Create<ConnectionRequest>(packet => {
            packet.ClientId = clientId;
            packet.Time = time;
            packet.UseSecurity = useSecurity;
        });
    }
}