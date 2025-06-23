using RakSharp.Protocol.Offline;
using System.Collections.Concurrent;
using RakSharp.Protocol.Online;
using RakSharp.Utils;
using BinaryReader = RakSharp.Binary.BinaryReader;

namespace RakSharp.Packet;

public static class DynamicPacketFactory {
    
    private static readonly List<Type> OfflinePacketTypes = [];
    private static readonly List<Type> OnlinePacketTypes = [];
    
    private static readonly ConcurrentDictionary<byte, (Type type, bool isOnline)> SuccessCache = new();
    private static readonly ConcurrentDictionary<(byte firstByte, Type type), bool> FailureCache = new();
    
    private static readonly Lock RegistrationLock = new();
    
    static DynamicPacketFactory() {
        RegisterDefaultPackets();
    }
    
    private static void RegisterDefaultPackets() {

        RegisterOfflinePacketType<UnconnectedPing>();
        RegisterOfflinePacketType<UnconnectedPong>();
        RegisterOfflinePacketType<OpenConnectionRequestFirst>();
        RegisterOfflinePacketType<OpenConnectionReplyFirst>();
        RegisterOfflinePacketType<OpenConnectionRequestSecond>();
        RegisterOfflinePacketType<OpenConnectionReplySecond>();
        RegisterOfflinePacketType<IncompatibleProtocolVersion>();
        
        RegisterOnlinePacketType<Datagram>();
        RegisterOnlinePacketType<Acknowledgement>();
        RegisterOnlinePacketType<NegativeAcknowledgement>();
    }
    
    public static void RegisterOfflinePacketType<T>() where T : OfflineMessage, new() {
        
        lock (RegistrationLock) {
            
            var type = typeof(T);
            if (!OfflinePacketTypes.Contains(type)) {
                OfflinePacketTypes.Add(type);
            }
        }
    }
    
    public static void RegisterOnlinePacketType<T>() where T : OnlineMessage, new() {
        
        lock (RegistrationLock) {
            
            var type = typeof(T);
            if (!OnlinePacketTypes.Contains(type)) {
                OnlinePacketTypes.Add(type);
            }
        }
    }
    
    public static OfflineMessage? CreateOfflineMessageFromBuffer(byte[] buffer) {
        
        if (buffer.Length == 0) {
            throw new PacketCorruptedException("Unknown", "Empty buffer");
        }
        
        var firstByte = buffer[0];
        if (SuccessCache.TryGetValue(firstByte, out var cached) && !cached.isOnline) {
            
            try {
                return TryCreateOfflinePacket(cached.type, buffer);
            } catch (RakSharpException.InvalidPacketIdException) {
                SuccessCache.TryRemove(firstByte, out _);
            }
        }
        
        var orderedTypes = OfflinePacketTypes.Where(type => !FailureCache.ContainsKey((firstByte, type))).OrderBy(type => FailureCache.Count(kvp => kvp.Key.type == type)).ToList();
        foreach (var packetType in orderedTypes) {
            
            try {
                
                var packet = TryCreateOfflinePacket(packetType, buffer);
                if (packet == null) 
                    continue;
                
                SuccessCache[firstByte] = (packetType, false);
                return packet;

            } catch (RakSharpException.InvalidPacketIdException) {
                FailureCache[(firstByte, packetType)] = true;
            } catch (Exception ex) {
                throw new PacketCorruptedException(packetType.Name, ex.Message);
            }
        }
        
        throw new RakSharpException.InvalidPacketIdException(0, firstByte, "Unknown offline packet type");
    }
    
    public static OnlineMessage? CreateOnlineMessageFromBuffer(byte[] buffer) {
        
        if (buffer.Length == 0) {
            throw new PacketCorruptedException("Unknown", "Empty buffer");
        }
        
        var firstByte = buffer[0];
        if (SuccessCache.TryGetValue(firstByte, out var cached) && cached.isOnline) {
            
            try {
                return TryCreateOnlinePacket(cached.type, buffer);
            } catch (RakSharpException.InvalidPacketIdException) {
                SuccessCache.TryRemove(firstByte, out _);
            }
        }
        
        var orderedTypes = OnlinePacketTypes.Where(type => !FailureCache.ContainsKey((firstByte, type))).OrderBy(type => FailureCache.Count(kvp => kvp.Key.type == type)).ToList();
        foreach (var packetType in orderedTypes) {

            try {

                var packet = TryCreateOnlinePacket(packetType, buffer);
                if (packet is null)
                    continue;

                SuccessCache[firstByte] = (packetType, true);
                return packet;

            } catch (RakSharpException.InvalidPacketIdException) {
                FailureCache[(firstByte, packetType)] = true;
            } catch (Exception ex) {
                Logger.LogError("Corrupted Packet Received: ", ex);
                return null;
            }
        }
        
        throw new RakSharpException.InvalidPacketIdException(0, firstByte, "Unknown online packet type");
    }
    
    private static OfflineMessage TryCreateOfflinePacket(Type packetType, byte[] buffer) {
        
        var reader = new BinaryReader(buffer);
        var packet = (OfflineMessage)Activator.CreateInstance(packetType)!;

        packet.ReadHeader(reader);
        reader.Position = 0;
        
        packet.Read(reader);
        return packet;
    }
    
    private static OnlineMessage TryCreateOnlinePacket(Type packetType, byte[] buffer) {
        
        var reader = new BinaryReader(buffer);
        var packet = (OnlineMessage)Activator.CreateInstance(packetType)!;
        
        packet.ReadHeader(reader);
        reader.Position = 0;
        
        packet.Read(reader);
        return packet;
    }
    
    public static object? CreatePacketFromBufferAuto(byte[] buffer) {
        
        if (buffer.Length == 0) {
            throw new PacketCorruptedException("Unknown", "Empty buffer");
        }
        
        var firstByte = buffer[0];
        if (SuccessCache.TryGetValue(firstByte, out var cached)) {
            
            try {
                
                if (cached.isOnline) {
                    return CreateOnlineMessageFromBuffer(buffer);
                }

                return CreateOfflineMessageFromBuffer(buffer);
            } catch (RakSharpException.InvalidPacketIdException) {
                SuccessCache.TryRemove(firstByte, out _);
            }
        }
        
        try {
            return CreateOfflineMessageFromBuffer(buffer);
        } catch (RakSharpException.InvalidPacketIdException) {
        }
        
        try {
            return CreateOnlineMessageFromBuffer(buffer);
        } catch (RakSharpException.InvalidPacketIdException) {
            Logger.LogError("I think you need to create this packet!", new RakSharpException.UnknownPacketIdException(firstByte));
            return null;
        }
    }
    
    public static void ClearCaches() {
        
        SuccessCache.Clear();
        FailureCache.Clear();
    }
    
    public static PacketFactoryStats GetStats() {
        
        return new PacketFactoryStats {
            RegisteredOfflineTypes = OfflinePacketTypes.Count,
            RegisteredOnlineTypes = OnlinePacketTypes.Count,
            SuccessCacheSize = SuccessCache.Count,
            FailureCacheSize = FailureCache.Count
        };
    }
    
    public class PacketFactoryStats {
        
        public int RegisteredOfflineTypes { get; set; }
        public int RegisteredOnlineTypes { get; set; }
        public int SuccessCacheSize { get; set; }
        public int FailureCacheSize { get; set; }
        
        public override string ToString() {
            return $"Offline: {RegisteredOfflineTypes}, Online: {RegisteredOnlineTypes}, " + $"Success Cache: {SuccessCacheSize}, Failure Cache: {FailureCacheSize}";
        }
    }
}


public static class EncapsulatedPacketFactory {
    
    private static readonly List<Type> EncapsulatedPacketTypes = [];
    
    private static readonly Lock RegistrationLock = new();

    static EncapsulatedPacketFactory() {
        RegisterDefaultEncapsulatedPackets();
    }
    
    private static void RegisterDefaultEncapsulatedPackets() {
        
        RegisterEncapsulatedPacketType<ConnectedPing>();
        RegisterEncapsulatedPacketType<ConnectedPong>();
        RegisterEncapsulatedPacketType<ConnectionRequest>();
        RegisterEncapsulatedPacketType<NewIncomingConnection>();
        RegisterEncapsulatedPacketType<Disconnect>();
    }

    public static void RegisterEncapsulatedPacketType<T>() where T : class, new() {
        
        lock (RegistrationLock) {
            
            var type = typeof(T);
            if (!EncapsulatedPacketTypes.Contains(type)) {
                EncapsulatedPacketTypes.Add(type);
            }
        }
    }

    public static object? CreateFromBuffer(byte[] buffer) {
        
        if (buffer is null || buffer.Length == 0) {
            throw new PacketCorruptedException("Unknown", "Empty buffer");
        }

        var firstByte = buffer[0];
        foreach (var packetType in EncapsulatedPacketTypes) {
            
            try {
                
                var packet = TryCreateEncapsulatedPacket(packetType, buffer);
                if (packet == null) 
                    continue;
                
                return packet;

            } catch (RakSharpException.InvalidPacketIdException) {
            } catch (Exception ex) {
                throw new PacketCorruptedException(packetType.Name, ex.Message);
            }
        }
        
        Logger.LogError("I think you need to create this packet!", new RakSharpException.UnknownPacketIdException(firstByte));
        return null;
    }

    private static object? TryCreateEncapsulatedPacket(Type packetType, byte[] buffer) {
        
        var reader = new BinaryReader(buffer);
        var packet = Activator.CreateInstance(packetType);
        
        if (packet == null) {
            return null;
        }
        
        dynamic dynamicPacket = packet;
        dynamicPacket.ReadHeader(reader);
        
        reader.Position = 0;
        dynamicPacket.Read(reader);
        
        return packet;
    }

    public static void Clear() {
        
        lock (RegistrationLock) {
            EncapsulatedPacketTypes.Clear();
        }
    }

    public static EncapsulatedPacketFactoryStats GetStats() {
        
        return new EncapsulatedPacketFactoryStats {
            RegisteredEncapsulatedTypes = EncapsulatedPacketTypes.Count
        };
    }

    public static int RegisteredTypeCount => EncapsulatedPacketTypes.Count;

    public class EncapsulatedPacketFactoryStats {
        
        public int RegisteredEncapsulatedTypes { get; set; }
        
        public override string ToString() {
            return $"Encapsulated: {RegisteredEncapsulatedTypes}";
        }
    }
}