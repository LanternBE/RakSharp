using System.Buffers;
using RakSharp.Packet;
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
        
        if (reader.Remaining < 3) {
            return null;
        }
        
        var packet = new EncapsulatedPacket();
        var flags = reader.ReadByte();
        
        packet.Reliability = (flags & ReliabilityFlags) >> ReliabilityShift;
        var hasSplit = (flags & SplitFlag) != 0;
        
        if (packet.Reliability > 7) {
            return null;
        }
        
        var rawLength = reader.ReadUnsignedShortBigEndian();
        if (rawLength > 8192 * 8) {
            return null;
        }
        
        var length = (rawLength + 7) / 8;
        if (length == 0)
            return null;
        
        var expectedHeaderSize = 0;
        if (PacketReliability.IsReliable(packet.Reliability))
            expectedHeaderSize += 3;
        if (PacketReliability.IsSequenced(packet.Reliability))
            expectedHeaderSize += 3;
        if (PacketReliability.IsSequencedOrOrdered(packet.Reliability))
            expectedHeaderSize += 4; // 3 + 1
        if (hasSplit)
            expectedHeaderSize += SplitInfoLength;
        
        if (expectedHeaderSize + length > reader.Remaining) {
            return null;
        }
        
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
    }
    
    public byte[] ToBytes() {
        
        var buffer = new byte[GetTotalLength()];
        var writer = new BinaryWriter(buffer);
        
        var flags = (byte)((Reliability << ReliabilityShift) | (SplitInfo != null ? SplitFlag : 0));
        writer.WriteByte(flags);

        writer.WriteShortBigEndian((short)(Buffer.Length * 8));
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

    public static EncapsulatedPacket Create(object packet, int reliability, int connectionReliableIndex, int connectionOrderedIndex, byte orderChannel = 0) {
        
        var reliableIndex = PacketReliability.IsReliable(reliability) ? connectionReliableIndex : 0;
        var orderIndex = PacketReliability.IsOrdered(reliability) ? connectionOrderedIndex : 0;

        var buffer = ArrayPool<byte>.Shared.Rent(1500);
        try {
            var writer = new BinaryWriter(buffer);

            switch (packet) {
                case OnlineMessage onlineMessage:
                    onlineMessage.Write(writer);
                    break;
                case OfflineMessage offlineMessage:
                    offlineMessage.Write(writer);
                    break;
                default:
                    throw new PacketClassException(packet.GetType().Name, "Unsupported packet type for EncapsulatedPacket creation.");
            }

            var data = writer.ToArray();
            return new EncapsulatedPacket {
                Reliability = reliability,
                MessageIndex = PacketReliability.IsReliable(reliability) ? reliableIndex : null,
                OrderIndex = PacketReliability.IsOrdered(reliability) ? orderIndex : null,
                OrderChannel = orderChannel,
                Buffer = data
            };
        } catch (Exception x) {
            Logger.LogError("Error creating EncapsulatedPacket:", x);
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        
        return null;
    }
    
    public static EncapsulatedPacket Create(byte[] buffer, int reliability, int connectionReliableIndex, int connectionOrderedIndex, byte orderChannel = 0) {
        
        return new EncapsulatedPacket {
            Reliability = reliability,
            MessageIndex = PacketReliability.IsReliable(reliability) ? connectionReliableIndex : null,
            OrderIndex = PacketReliability.IsOrdered(reliability) ? connectionOrderedIndex : null,
            OrderChannel = orderChannel,
            Buffer = buffer
        };
    }

    public int GetTotalLength() => GetHeaderLength() + Buffer.Length;
}