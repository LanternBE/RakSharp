using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class Disconnect : OnlineMessage {
    
    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.DisconnectionNotification;
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {
        
        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(Disconnect));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
    }

    protected override void ReadPayload(BinaryReader reader) {
    }
    
    public static (Disconnect packet, byte[] buffer) Create() {
        
        return OnlineMessage.Create<Disconnect>(_ => { });
    }
}