﻿using RakSharp.Packet;
using BinaryReader = RakSharp.Binary.BinaryReader;
using BinaryWriter = RakSharp.Binary.BinaryWriter;

namespace RakSharp.Protocol.Online;

public class Datagram : OnlineMessage {
    
    public override MessagesIdentifier.OnlineMessages PacketId => MessagesIdentifier.OnlineMessages.Datagram;
    
    internal const byte BitflagValid = 0x80;
    internal const byte BitflagAck = 0x40;
    internal const byte BitflagNak = 0x20;
    internal const int HeaderSize = 4;

    internal byte HeaderFlags { get; private set; }
    internal List<EncapsulatedPacket> Packets { get; private set; } = [];
    internal int SeqNumber { get; private set; }
    
    protected override void WriteHeader(BinaryWriter writer) {
        writer.WriteByte((byte)(BitflagValid | HeaderFlags));
    }

    protected internal override void ReadHeader(BinaryReader reader) {

        var packetId = reader.ReadByte();
        if (packetId != (int)PacketId) {
            throw new RakSharpException.InvalidPacketIdException((uint)PacketId, packetId, nameof(Datagram));
        }
    }

    protected override void WritePayload(BinaryWriter writer) {
        
        writer.WriteTriadLittleEndian(SeqNumber);
        foreach (var packet in Packets) {
            writer.Write(packet.ToBytes());
        }
    }
    
    protected override void ReadPayload(BinaryReader reader) {

        SeqNumber = reader.ReadTriadLittleEndian();
        while (!reader.IsAtEnd()) {

            var startPos = reader.Position;
            var packet = EncapsulatedPacket.FromBytes(reader);
        
            if (packet is not null) {
                Packets.Add(packet);
            }
            
            if (reader.Position == startPos) {
                break;
            }
        }
    }

    /*protected override void ReadPayload(BinaryReader reader) {
    
        SeqNumber = reader.ReadTriadLittleEndian();
        var consecutiveFailures = 0;
        
        const int maxConsecutiveFailures = 3;
        while (reader.Remaining > 3) {
            
            var currentPosition = reader.Position;
            try {
                
                var packet = EncapsulatedPacket.FromBytes(reader);
                if (packet is not null) {
                    Packets.Add(packet);
                    consecutiveFailures = 0;
                } else {
                    consecutiveFailures++;
                    if (consecutiveFailures >= maxConsecutiveFailures) {
                        break;
                    }
                    
                    if (reader.Remaining > 0) {
                        reader.Position = currentPosition + 1;
                    }
                }
            } catch (Exception ex) {
                
                consecutiveFailures++;
                if (consecutiveFailures >= maxConsecutiveFailures) {
                    break;
                }
                
                if (reader.Remaining > 0) {
                    reader.Position = currentPosition + 1;
                }
            }
            
            if (reader.Position == currentPosition && reader.Remaining > 0) {
                reader.Position++;
            }
        }
    }*/

    public static (Datagram packet, byte[] buffer) Create(byte headerFlags, List<EncapsulatedPacket> packets, int seqNumber) {
        
        return OnlineMessage.Create<Datagram>(packet => {
            packet.HeaderFlags = headerFlags;
            packet.Packets = packets;
            packet.SeqNumber = seqNumber;
        });
    }
}