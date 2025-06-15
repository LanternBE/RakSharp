using System.Data;
using System.Net;

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

    public ClientSession(long clientId, IPEndPoint remoteEndPoint) {
        
        ClientId = clientId;
        RemoteEndPoint = remoteEndPoint;

        State = ConnectionState.Executing;
        ConnectedTime = DateTime.UtcNow;
        
        Logger.LogInfo($"ClientSession created for ({remoteEndPoint})");
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

    public void Disconnect(string reason = "Disconnected") {
        
        Logger.LogInfo($"ClientSession disconnected for ({RemoteEndPoint}): {reason}");
        Dispose();
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}