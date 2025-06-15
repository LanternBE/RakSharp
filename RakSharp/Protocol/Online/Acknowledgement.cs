using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class Acknowledgement : OnlineMessage {
    
    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.Acknowledgement;
    
    internal const byte RecordTypeRange = 0;
    internal const byte RecordTypeSingle = 1;

    internal List<int> Packets { get; private set; } = [];
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)PacketId);
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(Acknowledgement));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {

        if (Packets.Count is 0) {
            writer.WriteShortBigEndian(0);
            return;
        }
        
        Packets.Sort();
        var records = 0;

        var pointer = 1;
        var start = Packets[0];
        
        var last = Packets[0];
        var payloadBuffer = new List<byte>();

        while (pointer < Packets.Count) {
            
            var current = Packets[pointer++];
            var diff = current - last;

            switch (diff) {
                case 1:
                    last = current;
                    break;
                case > 1: {
                    
                    if (start == last) {
                        payloadBuffer.Add(RecordTypeSingle);
                        payloadBuffer.AddRange(WriteTriadLittleEndian(start));
                    } else {
                        payloadBuffer.Add(RecordTypeRange);
                        payloadBuffer.AddRange(WriteTriadLittleEndian(start));
                        payloadBuffer.AddRange(WriteTriadLittleEndian(last));
                    }
                    records++;

                    start = last = current;
                    break;
                }
            }
        }
        
        if (start == last) {
            payloadBuffer.Add(RecordTypeSingle);
            payloadBuffer.AddRange(WriteTriadLittleEndian(start));
        } else {
            payloadBuffer.Add(RecordTypeRange);
            payloadBuffer.AddRange(WriteTriadLittleEndian(start));
            payloadBuffer.AddRange(WriteTriadLittleEndian(last));
        }
        records++;

        writer.WriteShortBigEndian((short)records);
        writer.Write(payloadBuffer.ToArray());
    }

    protected override void ReadPayload(BinaryReader reader) {
        
        var recordCount = reader.ReadShortBigEndian();
        Packets.Clear();
        var packetsAdded = 0;
        
        const int maxPackets = 4096;
        const int maxRangeLength = 512;
        
        for (var i = 0; i < recordCount && !reader.IsAtEnd() && packetsAdded < maxPackets; i++) {
            
            if (reader.ReadByte() == RecordTypeRange) {
                
                var start = reader.ReadTriadLittleEndian();
                var end = reader.ReadTriadLittleEndian();

                if (end - start > maxRangeLength) {
                    end = start + maxRangeLength;
                }

                for (var c = start; c <= end && packetsAdded < maxPackets; c++) {
                    Packets.Add(c);
                    packetsAdded++;
                }
            } else {
                var packet = reader.ReadTriadLittleEndian();
                Packets.Add(packet);
                packetsAdded++;
            }
        }
    }
    
    public static byte[] WriteTriadLittleEndian(int value) => [(byte)(value & 0xFF), (byte)((value >> 8) & 0xFF), (byte)((value >> 16) & 0xFF)];

    public static (Acknowledgement packet, byte[] buffer) Create(List<int> packets) {
        
        return OnlineMessage.Create<Acknowledgement>(packet => {
            packet.Packets = packets;
        });
    }
}