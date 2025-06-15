namespace RakSharp.Protocol;

public static class PacketReliability {

    public const int Unreliable = 0;
    public const int UnreliableSequenced = 1;
    public const int Reliable = 2;
    public const int ReliableOrdered = 3;
    public const int ReliableSequenced = 4;
    
    public const int UnreliableWithAckReceipt = 5;
    public const int ReliableWithAckReceipt = 6;
    public const int ReliableOrderedWithAckReceipt = 7;

    public const int MaxOrderChannels = 32;

    public static bool IsReliable(int reliability) => reliability is Reliable or ReliableOrdered or ReliableSequenced or ReliableWithAckReceipt or ReliableOrderedWithAckReceipt;
    public static bool IsSequenced(int reliability) => reliability is UnreliableSequenced or ReliableSequenced;
    public static bool IsOrdered(int reliability) => reliability is ReliableOrdered or ReliableOrderedWithAckReceipt;
    public static bool IsSequencedOrOrdered(int reliability) => reliability is UnreliableSequenced or ReliableOrdered or ReliableSequenced or ReliableOrderedWithAckReceipt;
}

public static class PacketReliabilityExtensions {
    
    public static bool IsReliable(this int reliability) => reliability is PacketReliability.Reliable or PacketReliability.ReliableOrdered or PacketReliability.ReliableSequenced or PacketReliability.ReliableWithAckReceipt or PacketReliability.ReliableOrderedWithAckReceipt;
    public static bool IsOrdered(this int reliability) => reliability is PacketReliability.ReliableOrdered or PacketReliability.ReliableOrderedWithAckReceipt;
}
