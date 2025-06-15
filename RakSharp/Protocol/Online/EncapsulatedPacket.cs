using RakSharp.Utils;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class EncapsulatedPacket {
    
    private const int ReliabilityShift = 5;
    private const int ReliabilityFlags = 0b111 << ReliabilityShift;
    private const int SplitFlag = 0b00010000;
    public const int SplitInfoLength = 10;

    public int Reliability { get; set; }
    public int? MessageIndex { get; set; }
    public int? SequenceIndex { get; set; }
    public int? OrderIndex { get; set; }
    public int? OrderChannel { get; set; }
    public SplitPacketInfo? SplitInfo { get; set; }
    public byte[] Buffer { get; set; } = [];
    public int? IdentifierACK { get; set; } = null;

    public static EncapsulatedPacket? FromBytes(BinaryReader reader) {

        try {
        
            var packet = new EncapsulatedPacket();
            var flags = reader.ReadByte();
            
            packet.Reliability = (flags & ReliabilityFlags) >> ReliabilityShift;
            var hasSplit = (flags & SplitFlag) != 0;
            
            var rawLength = reader.ReadUnsignedShortBigEndian();
            var length = (int)Math.Ceiling(rawLength / 8.0);

            if (length == 0)
                return null;
            
            if (PacketReliability.IsReliable(packet.Reliability))
                packet.MessageIndex = reader.ReadTriadLittleEndian();

            if (PacketReliability.IsSequenced(packet.Reliability))
                packet.SequenceIndex = reader.ReadTriadLittleEndian();

            if (PacketReliability.IsSequencedOrOrdered(packet.Reliability)) {
                packet.OrderIndex = reader.ReadTriadLittleEndian();
                packet.OrderChannel = reader.ReadByte();
            }

            if (hasSplit) {
                var splitCount = reader.ReadIntBigEndian();
                var splitId = reader.ReadShortBigEndian();
                var splitIndex = reader.ReadIntBigEndian();
                packet.SplitInfo = new SplitPacketInfo(splitId, splitIndex, splitCount);
            }

            packet.Buffer = reader.ReadBytes(length);
            return packet;
            
        } catch (Exception ex) {
            return null;
        }    
    }
    
    public byte[] ToBytes() {
        
        var buffer = new byte[GetTotalLength()];
        var writer = new BinaryWriter(buffer);
        
        var flags = (byte)((Reliability << ReliabilityShift) | (SplitInfo != null ? SplitFlag : 0));
        writer.WriteByte(flags);
        
        writer.WriteShortBigEndian((short)(Buffer.Length << 3));
        if (PacketReliability.IsReliable(Reliability))
            writer.WriteTriadLittleEndian(MessageIndex ?? 0);
        
        if (PacketReliability.IsSequenced(Reliability))
            writer.WriteTriadLittleEndian(SequenceIndex ?? 0);
        
        if (PacketReliability.IsSequencedOrOrdered(Reliability)) {
            writer.WriteTriadLittleEndian(OrderIndex ?? 0);
            writer.WriteByte((byte)(OrderChannel ?? 0));
        }
        
        if (SplitInfo != null) {
            writer.WriteIntBigEndian(SplitInfo.TotalPartCount);
            writer.WriteShortBigEndian((short)SplitInfo.Id);
            writer.WriteIntBigEndian(SplitInfo.PartIndex);
        }
        
        writer.Write(Buffer);
        return writer.ToArray();
    }

    public int GetHeaderLength() {
        
        return 1 + 2 + (PacketReliability.IsReliable(Reliability) ? 3 : 0) 
                     + (PacketReliability.IsSequenced(Reliability) ? 3 : 0) 
                     + (PacketReliability.IsSequencedOrOrdered(Reliability) ? 3 + 1 : 0) 
                     + (SplitInfo != null ? SplitInfoLength : 0);
    }

    public int GetTotalLength() => GetHeaderLength() + Buffer.Length;
}