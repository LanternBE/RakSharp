using System.Net;
using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class ConnectionRequestAccepted : OnlineMessage {
    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.ConnectionRequestAccepted;

    public IPEndPoint? ClientAddress { get; private set; } = null;
    public List<IPEndPoint>? SystemAddresses { get; private set; } = null;
    public long SendPingTime { get; private set; }
    public long SendPongTime { get; private set; }
    
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
        writer.WriteIpEndPoint(ClientAddress);
        writer.WriteShortBigEndian(0);

        var dummy = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
        for (var i = 0; i < 20; i++) {
            var addr = i < SystemAddresses?.Count ? SystemAddresses[i] : dummy;
            writer.WriteIpEndPoint(addr);
        }
        
        writer.WriteLongBigEndian(SendPingTime);
        writer.WriteLongBigEndian(SendPongTime);
    }

    protected override void ReadPayload(BinaryReader reader) {
        ClientAddress = reader.ReadIpEndPoint();
        reader.ReadShortBigEndian();

        var dummy = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
        SystemAddresses = [];
        
        for (var i = 0; i < 20; i++) {
            SystemAddresses.Add(reader.Remaining >= 16 ? reader.ReadIpEndPoint() : dummy);
        }
        
        SendPingTime = reader.ReadLongBigEndian();
        SendPongTime = reader.ReadLongBigEndian();
    }
}