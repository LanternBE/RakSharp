using System.Data;
using System.Net;
using RakSharp.Protocol.Online;

namespace RakSharp.Utils.Sessions;

public class ClientSession : IDisposable {
    
    public long ClientId { get; init; }
    public IPEndPoint RemoteEndPoint { get; init; }
    public int ProtocolVersion { get; set; }
    
    public ConnectionState State { get; set; }
    public bool Compression { get; set; }
    public ushort CompressionThreshold { get; set; }
    
    public int NextSequenceNumber { get; set; }
    public int NextReliableIndex { get; set; }
    public int NextOrderedIndex { get; set; }
    public int LastSequenceNumber { get; set; }
    
    public DateTime LastPacketTime { get; set; } = DateTime.UtcNow;
    public DateTime ConnectedTime { get; set; }
    
    private readonly Dictionary<int, (object Packet, DateTime SentTime)> _pendingReliablePackets = new();

    public ClientSession(long clientId, IPEndPoint remoteEndPoint) {
        
        ClientId = clientId;
        RemoteEndPoint = remoteEndPoint;

        State = ConnectionState.Executing;
        ConnectedTime = DateTime.UtcNow;
        
        Logger.LogInfo($"ClientSession created for ({remoteEndPoint})");
    }
    
    public void TrackReliablePacket(int sequenceNumber, object packet) {
        _pendingReliablePackets[sequenceNumber] = (packet, DateTime.UtcNow);
    }

    public int GetNextSequenceNumber() {
        return NextSequenceNumber++;
    }
    
    public int GetNextReliableIndex() {
        return NextReliableIndex++;
    }
    
    public int GetNextOrderedIndex() {
        return NextOrderedIndex++;
    }
    
    public void UpdateLastPacketTime() {
        LastPacketTime = DateTime.UtcNow;
    }
    
    public void HandleAcknowledgement(Acknowledgement ack) {
        
        foreach (var sequenceNumber in ack.Packets) {
            if (_pendingReliablePackets.Remove(sequenceNumber)) {
                Logger.LogDebug($"ACK: Confirmed reliable packet {sequenceNumber} from {RemoteEndPoint}");
            } else {
                Logger.LogWarn($"ACK: Got unknown sequence number {sequenceNumber} from {RemoteEndPoint}");
            }
        }
    }

    public void Disconnect(string reason = "Disconnected") {
        
        Logger.LogInfo($"ClientSession disconnected for ({RemoteEndPoint}): {reason}");
        Dispose();
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}