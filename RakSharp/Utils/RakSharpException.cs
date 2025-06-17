namespace RakSharp;

public abstract partial class RakSharpException : Exception {
    
    protected RakSharpException(string message) : base(message) { }
    protected RakSharpException(string message, Exception innerException) : base(message, innerException) { }
}

public partial class RakSharpException {
    
    /// <summary>
    /// Exception thrown when a packet ID doesn't match the expected value
    /// </summary>
    public class InvalidPacketIdException : RakSharpException {
        
        public uint ExpectedId { get; }
        public int ActualId { get; }
        public string PacketType { get; }

        public InvalidPacketIdException(uint expectedId, int actualId, string packetType) : base($"Invalid packet ID for {packetType}: expected {expectedId}, got {actualId}") {
            
            ExpectedId = expectedId;
            ActualId = actualId;
            PacketType = packetType;
        }

        public InvalidPacketIdException(uint expectedId, int actualId, string packetType, Exception innerException) : base($"Invalid packet ID for {packetType}: expected {expectedId}, got {actualId}", innerException) {
            
            ExpectedId = expectedId;
            ActualId = actualId;
            PacketType = packetType;
        }
    }
    
    /// <summary>
    /// Exception thrown when a magic is wrong
    /// </summary>
    public class InvalidMagicException : RakSharpException {

        public InvalidMagicException() : base("Invalid magic!") { }
        public InvalidMagicException(Exception innerException) : base("Invalid magic!", innerException) { }
    }
    
    /// <summary>
    /// Exception thrown when a write operation would exceed the buffer size
    /// </summary>
    public class BufferOverflowException : RakSharpException {
        
        public int BufferSize { get; }
        public int CurrentPosition { get; }
        public int AttemptedWriteSize { get; }
        public int RequiredSize { get; }

        public BufferOverflowException(int bufferSize, int currentPosition, int attemptedWriteSize) : base($"Write operation exceeds buffer size: buffer size {bufferSize}, current position {currentPosition}, attempted write size {attemptedWriteSize}, required size {currentPosition + attemptedWriteSize}") {
            
            BufferSize = bufferSize;
            CurrentPosition = currentPosition;
            AttemptedWriteSize = attemptedWriteSize;
            RequiredSize = currentPosition + attemptedWriteSize;
        }

        public BufferOverflowException(int bufferSize, int currentPosition, int attemptedWriteSize, string operation) : base($"Write operation '{operation}' exceeds buffer size: buffer size {bufferSize}, current position {currentPosition}, attempted write size {attemptedWriteSize}, required size {currentPosition + attemptedWriteSize}") {
            
            BufferSize = bufferSize;
            CurrentPosition = currentPosition;
            AttemptedWriteSize = attemptedWriteSize;
            RequiredSize = currentPosition + attemptedWriteSize;
        }
    }
}

/// <summary>
/// Exception thrown when packet data is corrupted or malformed
/// </summary>
public class PacketCorruptedException : RakSharpException {
    
    public string PacketType { get; }
    public byte[]? RawData { get; }

    public PacketCorruptedException(string packetType, string reason) : base($"Packet '{packetType}' is corrupted: {reason}") {
        PacketType = packetType;
    }

    public PacketCorruptedException(string packetType, string reason, byte[] rawData) : base($"Packet '{packetType}' is corrupted: {reason}") {
        
        PacketType = packetType;
        RawData = rawData;
    }
}

/// <summary>
/// Exception thrown when packet class is invalid
/// </summary>
public class PacketClassException(string clazz, string reason)
    : RakSharpException($"Packet class '{clazz}' is corrupted: {reason}");