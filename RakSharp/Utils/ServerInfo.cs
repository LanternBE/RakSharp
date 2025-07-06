namespace RakSharp.Utils;

public class ServerInfo {

    public string Edition { get; set; } = "MCPE";
    public string ServerName { get; set; } = "Lantern";
    public int ProtocolVersion { get; set; } = 818;
    public string ServerVersion { get; set; } = "1.21.92";
    public int OnlinePlayers { get; set; }
    public int MaxPlayers { get; set; } = 20;
    public long ServerGuid { get; set; } = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
    public string Motd { get; set; } = "A server powered by Lantern";
    public string GameMode { get; set; } = "Survival";
    public int GameModeId { get; set; } = 1;
    public int PortIpv4 { get; set; } = 19132;
    public int PortIpv6 { get; set; } = 19133;

    public override string ToString() {
        return $"{Edition};{ServerName};{ProtocolVersion};{ServerVersion};{OnlinePlayers};{MaxPlayers};{ServerGuid};{Motd};{GameMode};{GameModeId};{PortIpv4};{PortIpv6};";
    }
}