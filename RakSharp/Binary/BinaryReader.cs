using System.Buffers.Binary;
using System.Net;
using System.Text;
using RakSharp.Protocol;

namespace RakSharp.Binary;

public class BinaryReader(byte[] buffer) {
    
    public byte[] Buffer { get; set; } = buffer;
    public int Position { get; set; }
    public int Remaining => Buffer.Length - Position;
    
    public bool IsAtEnd() => Position >= Buffer.Length;
    public byte ReadByte() => Buffer[Position++];
    public bool ReadBoolean() => ReadByte() is 1;
    public byte[] ReadRemainingBytes() => ReadBytes(Buffer.Length - Position);
    
    public byte[] GetReadBytes() {
        
        var result = new byte[Position];
        Buffer.AsSpan(0, Position).CopyTo(result);
        
        return result;
    }
    
    public byte[] ReadBytes(int length) {
        
        if (length > Remaining)
            throw new ArgumentOutOfRangeException(nameof(length), $"Not enough data to read {length} bytes, only {Remaining} remaining.");

        var result = new byte[length];
        Buffer.AsSpan(Position, length).CopyTo(result);
        
        Position += length;
        return result;
    }
    
    public short ReadShortLittleEndian() {
        
        var value = BinaryPrimitives.ReadInt16LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfShort)]);
        Position += Info.SizeOfShort;
        
        return value;
    }
    
    public short ReadShortBigEndian() {

        var value = BinaryPrimitives.ReadInt16BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfShort)]);
        Position += Info.SizeOfShort;
        
        return value;
    }
    
    public ushort ReadUnsignedShortLittleEndian() {

        var value = BinaryPrimitives.ReadUInt16LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfShort)]);
        Position += Info.SizeOfShort;
        
        return value;
    }
    
    public ushort ReadUnsignedShortBigEndian() {

        var value = BinaryPrimitives.ReadUInt16BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfShort)]);
        Position += Info.SizeOfShort;
        
        return value;
    }

    public int ReadIntLittleEndian() {
        
        if (Position + 4 > Buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(Position), "Not enough data to read Int32.");
        
        var value = BinaryPrimitives.ReadInt32LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfInt)]);
        Position += Info.SizeOfInt;
        
        return value;
    }

    public int ReadIntBigEndian() {
    
        if (Position + Info.SizeOfInt > Buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(Position), "Not enough data to read Int32.");

        var value = BinaryPrimitives.ReadInt32BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfInt)]);
        Position += Info.SizeOfInt;
    
        return value;
    }
    
    public uint ReadUnsignedIntLittleEndian() {

        var value = BinaryPrimitives.ReadUInt32LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfInt)]);
        Position += Info.SizeOfInt;
        
        return value;
    }
    
    public uint ReadUnsignedIntBigEndian() {

        var value = BinaryPrimitives.ReadUInt32BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfInt)]);
        Position += Info.SizeOfInt;
        
        return value;
    }
    
    public long ReadLongLittleEndian() {

        var value = BinaryPrimitives.ReadInt64LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfLong)]);
        Position += Info.SizeOfLong;
        
        return value;
    }
    
    public long ReadLongBigEndian() {

        var value = BinaryPrimitives.ReadInt64BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfLong)]);
        Position += Info.SizeOfLong;
        
        return value;
    }
    
    public ulong ReadUnsignedLongLittleEndian() {

        var value = BinaryPrimitives.ReadUInt64LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfLong)]);
        Position += Info.SizeOfLong;
        
        return value;
    }
    
    public ulong ReadUnsignedLongBigEndian() {
        
        var value = BinaryPrimitives.ReadUInt64BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfLong)]);
        Position += Info.SizeOfLong;
        
        return value;
    }
    
    public float ReadFloatLittleEndian() {
        
        var value = BinaryPrimitives.ReadSingleLittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfFloat)]);
        Position += Info.SizeOfFloat;
        
        return value;
    }
    
    public float ReadFloatBigEndian() {
        
        var value = BinaryPrimitives.ReadSingleBigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfFloat)]);
        Position += Info.SizeOfFloat;
        
        return value;
    }

    public double ReadDoubleLittleEndian() {
        
        var value = BinaryPrimitives.ReadDoubleLittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfDouble)]);
        Position += Info.SizeOfDouble;
        
        return value;
    }
    
    public double ReadDoubleBigEndian() {
        
        var value = BinaryPrimitives.ReadDoubleBigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfDouble)]);
        Position += Info.SizeOfDouble;
        
        return value;
    }
    
    public int ReadTriadLittleEndian() {
        
        if (Remaining < 3)
            throw new ArgumentOutOfRangeException(nameof(ReadTriadLittleEndian), "Not enough data to read a triad (3 bytes).");

        var value = Buffer[Position] | (Buffer[Position + 1] << 8) | (Buffer[Position + 2] << 16);
        Position += 3;
        
        return value;
    }
    
    public string ReadString() {
        
        var length = ReadUnsignedShortBigEndian();
        var value = Encoding.UTF8.GetString(Buffer, Position, length);
        Position += length;
        
        return value;
    }

    public void ReadMagic() {
        
        if (!Buffer[Position..(Position + MessagesIdentifier.Magic.Length)].SequenceEqual(MessagesIdentifier.Magic)) {
            throw new RakSharpException.InvalidMagicException();
        }

        Position += MessagesIdentifier.Magic.Length;
    }
    
    public IPEndPoint ReadIpEndPoint() {
        
        var family = ReadByte();
        
        IPAddress address;
        int port;

        switch (family) {
            
            case 4: {
                
                address = IPAddress.Parse(
                    $"{(byte) ~ReadByte()}." + 
                    $"{(byte) ~ReadByte()}." + 
                    $"{(byte) ~ReadByte()}." + 
                    $"{(byte) ~ReadByte()}"
                );

                port = ReadUnsignedShortBigEndian();
                break;
            }

            case 6: {
                
                Position += Info.SizeOfShort;
                port = ReadUnsignedShortBigEndian();
                Position += Info.SizeOfLong;
                address = new IPAddress(ReadBytes(16));
                break;
            }

            default:
                return new IPEndPoint(IPAddress.Loopback, 19132);
        }

        return new IPEndPoint(address, port);
    }
    
    public int ReadVarInt() {
        
        var result = 0;
        var shift = 0;

        while (true) {
            
            if (Position >= Buffer.Length)
                throw new InvalidOperationException("Unexpected end of buffer while reading VarInt.");

            var b = Buffer[Position++];
            result |= (b & 127) << shift;

            shift += 7;
            if ((b & 128) == 0)
                break;

            if (shift > 35)
                throw new FormatException("VarInt is too large.");
        }
        
        return result;
    }
    
    
    public uint ReadVarUInt() {
        
        uint result = 0;
        for (var i = 0; i <= 28; i += 7) {
        
            if (Position >= Buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(Position), "No bytes left in buffer");

            var b = Buffer[Position++];
            result |= (uint)(b & 127) << i;
        
            if ((b & 128) == 0)
                return result;
        }
    
        throw new FormatException("VarInt did not terminate after 5 bytes!");
    }
}