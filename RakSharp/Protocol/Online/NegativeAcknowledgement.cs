using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class NegativeAcknowledgement : OnlineMessage {
    
    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.NegativeAcknowledgement;

    public (int Start, int End)[]? Records { get; set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {
        
        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(NegativeAcknowledgement));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        if (Records == null) {
            return;
        }
        
        writer.WriteShortBigEndian((short)Records.Length);
        foreach (var (start, end) in Records) {
            
            if (start == end) {
                writer.WriteBoolean(true);
                writer.WriteTriadLittleEndian(start);
            } else {
                writer.WriteBoolean(false);
                writer.WriteTriadLittleEndian(start);
                writer.WriteTriadLittleEndian(end);
            }
        }
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        var recordCount = reader.ReadShortBigEndian();
        Records = new (int Start, int End)[recordCount];

        for (var index = 0; index < Records.Length; index++) {
            
            var isSingle = reader.ReadBoolean();
            if (isSingle) {
                var sequenceNumber = reader.ReadTriadLittleEndian();
                Records[index] = (sequenceNumber, sequenceNumber);
            } else {
                var start = reader.ReadTriadLittleEndian();
                var end = reader.ReadTriadLittleEndian();
                Records[index] = (start, end);
            }
        }
    }
    
    public static (NegativeAcknowledgement packet, byte[] buffer) Create((int Start, int End)[] records) {
        
        return OnlineMessage.Create<NegativeAcknowledgement>(packet => {
            packet.Records = records;
        });
    }
}