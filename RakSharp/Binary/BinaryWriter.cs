using System.Buffers.Binary;
using System.Net;
using System.Text;
using RakSharp.Protocol;

namespace RakSharp.Binary;

public class BinaryWriter(byte[] buffer) {

    public byte[] Buffer { get; } = buffer;
    public int Position { get; set; }
    public byte[] ToArray() => Buffer[..Position];

    public void WriteByte(byte value) => Buffer[Position++] = value;
    public void WriteBoolean(bool value) => Buffer[Position++] = (byte)(value ? 1 : 0);
    
    public void Write(byte[] bytes) {
        
        if (Position + bytes.Length > Buffer.Length)
            throw new RakSharpException.BufferOverflowException(Buffer.Length, Position, Position + bytes.Length, "Write");
        
        Array.Copy(bytes, 0, Buffer, Position, bytes.Length);
        Position += bytes.Length;
    }
    
    public void WriteShortLittleEndian(short value) {
        
        BinaryPrimitives.WriteInt16LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfShort)], value);
        Position += Info.SizeOfShort;
    }

    public void WriteShortBigEndian(short value) {
        
        BinaryPrimitives.WriteInt16BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfShort)], value);
        Position += Info.SizeOfShort;
    }

    public void WriteUnsignedShortLittleEndian(ushort value) {
        
        BinaryPrimitives.WriteUInt16LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfShort)], value);
        Position += Info.SizeOfShort;
    }

    public void WriteUnsignedShortBigEndian(ushort value) {
        
        BinaryPrimitives.WriteUInt16BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfShort)], value);
        Position += Info.SizeOfShort;
    }

    public void WriteFloatLittleEndian(float value) {
        
        BinaryPrimitives.WriteSingleLittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfFloat)], value);
        Position += Info.SizeOfFloat;
    }
    
    public void WriteFloatBigEndian(float value) {
        
        BinaryPrimitives.WriteSingleBigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfFloat)], value);
        Position += Info.SizeOfFloat;
    }

    public void WriteIntLittleEndian(int value) {
        
        BinaryPrimitives.WriteInt32LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfInt)], value);
        Position += Info.SizeOfInt;
    }

    public void WriteIntBigEndian(int value) {
        
        BinaryPrimitives.WriteInt32BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfInt)], value);
        Position += Info.SizeOfInt;
    }

    public void WriteUnsignedIntLittleEndian(uint value) {
        
        BinaryPrimitives.WriteUInt32LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfInt)], value);
        Position += Info.SizeOfInt;
    }

    public void WriteUnsignedIntBigEndian(uint value) {
        
        BinaryPrimitives.WriteUInt32BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfInt)], value);
        Position += Info.SizeOfInt;
    }

    public void WriteLongLittleEndian(long value) {
        
        BinaryPrimitives.WriteInt64LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfLong)], value);
        Position += Info.SizeOfLong;
    }

    public void WriteLongBigEndian(long value) {
        
        BinaryPrimitives.WriteInt64BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfLong)], value);
        Position += Info.SizeOfLong;
    }

    public void WriteUnsignedLongLittleEndian(ulong value) {
        
        BinaryPrimitives.WriteUInt64LittleEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfLong)], value);
        Position += Info.SizeOfLong;
    }

    public void WriteUnsignedLongBigEndian(ulong value) {
        
        BinaryPrimitives.WriteUInt64BigEndian(Buffer.AsSpan()[Position..(Position + Info.SizeOfLong)], value);
        Position += Info.SizeOfLong;
    }
    
    public void WriteTriadLittleEndian(int value) {
        
        if (value is < 0 or > 0xFFFFFF)
            throw new ArgumentOutOfRangeException(nameof(value), "Triad must be between 0 and 16777215 (0xFFFFFF).");

        if (Position + 3 > Buffer.Length)
            throw new InvalidOperationException("Write exceeds buffer size.");

        Buffer[Position++] = (byte)(value & 0xFF);
        Buffer[Position++] = (byte)((value >> 8) & 0xFF);
        Buffer[Position++] = (byte)((value >> 16) & 0xFF);
    }

    public void WriteString(string value) {
        
        var encoded = Encoding.UTF8.GetBytes(value);
        if (encoded.Length > ushort.MaxValue)
            throw new RakSharpException.BufferOverflowException(Buffer.Length, Position, encoded.Length, "WriteString");
        
        WriteUnsignedShortBigEndian((ushort)encoded.Length);
        if (Position + encoded.Length > Buffer.Length)
            throw new RakSharpException.BufferOverflowException(Buffer.Length, Position, Position + encoded.Length, "WriteString");
        
        Array.Copy(encoded, 0, Buffer, Position, encoded.Length);
        Position += encoded.Length;
    }

    public void WriteMagic() {
        
        if (Position + MessagesIdentifier.Magic.Length > Buffer.Length)
            throw new RakSharpException.BufferOverflowException(Buffer.Length, Position, MessagesIdentifier.Magic.Length, "WriteMagic");
        
        Array.Copy(MessagesIdentifier.Magic, 0, Buffer, Position, MessagesIdentifier.Magic.Length);
        Position += MessagesIdentifier.Magic.Length;
    }
    
    public void WriteIpEndPoint(IPEndPoint? endPoint) {
        
        switch (endPoint?.Address.AddressFamily) {
            
            case System.Net.Sockets.AddressFamily.InterNetwork: {
                
                WriteByte(4);
                var addressBytes = endPoint.Address.GetAddressBytes();
                
                foreach (var b in addressBytes) {
                    WriteByte((byte)~b);
                }

                WriteUnsignedShortBigEndian((ushort)endPoint.Port);
                break;
            }
            
            case System.Net.Sockets.AddressFamily.InterNetworkV6: {
                
                WriteByte(6); 
                WriteUnsignedShortBigEndian(23);
                
                WriteUnsignedShortBigEndian((ushort)endPoint.Port);
                WriteUnsignedLongBigEndian(0);

                var addressBytes = endPoint.Address.GetAddressBytes();
                if (addressBytes.Length != 16)
                    throw new ArgumentException("IPv6 address must be 16 bytes long");

                Write(addressBytes);
                break;
            }
            
            default:
                throw new InvalidOperationException("Unsupported address family");
        }
    }
    
    public void WriteVarUInt(long value) {
        
        while ((value & 4294967168) != 0) {
            WriteByte((byte)((value & 127) | 128));
            value >>= 7;
        }

        WriteByte((byte)(value & 127));
    }
    
    public void WriteVarInt(int value) {
        
        var zigzag = (uint)((value << 1) ^ (value >> 31));
        while ((zigzag & 4294967168) != 0) {
            WriteByte((byte)((zigzag & 127) | 128));
            zigzag >>= 7;
        }

        WriteByte((byte)(zigzag & 127));
    }
    
    public void WriteVarIntSimple(int value) {
        
        var unsignedValue = (uint)value;
        while ((unsignedValue & 4294967168) != 0) {
            WriteByte((byte)((unsignedValue & 127) | 128));
            unsignedValue >>= 7;
        }

        WriteByte((byte)(unsignedValue & 127));
    }
}