using RakSharp.Protocol.Offline;
using BinaryReader = RakSharp.Binary.BinaryReader;

namespace RakSharp.Packet;

public static class PacketFactory {

    public static OfflineMessage? CreateOfflineMessageFromBuffer(byte[] buffer) {

        if (buffer.Length == 0) {
            throw new PacketCorruptedException("Unknown", "Empty buffer");
        }
            
        var packetTypes = new[] {
            typeof(UnconnectedPing),
            typeof(UnconnectedPong),
            typeof(OpenConnectionRequestFirst),
            typeof(OpenConnectionReplyFirst),
            typeof(OpenConnectionRequestSecond),
            typeof(OpenConnectionReplySecond),
            typeof(IncompatibleProtocolVersion)
        };
        
        foreach (var packetType in packetTypes) {
            
            try {
                var reader = new BinaryReader(buffer);
                var packet = (OfflineMessage)Activator.CreateInstance(packetType);
                
                packet?.ReadHeader(reader);
                
                reader.Position = 0;
                packet?.Read(reader);
                
                return packet;
            } catch (RakSharpException.InvalidPacketIdException) {
            } catch (Exception ex) {
                throw new PacketCorruptedException(packetType.Name, ex.Message);
            }
        }
        
        throw new RakSharpException.InvalidPacketIdException(0, buffer.Length > 0 ? buffer[0] : 0, "Unknown packet type");
    }
}