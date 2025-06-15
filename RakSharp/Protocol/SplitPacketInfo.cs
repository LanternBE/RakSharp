namespace RakSharp.Protocol;

public sealed class SplitPacketInfo(int id, int partIndex, int totalPartCount) {
    
    public int Id { get; } = id;
    public int PartIndex { get; } = partIndex;
    public int TotalPartCount { get; } = totalPartCount;
}