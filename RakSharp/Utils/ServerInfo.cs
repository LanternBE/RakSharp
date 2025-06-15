namespace RakSharp.Protocol;

public class ServerInfo {

    public string Edition { get; set; } = "MCPE";
    public string ServerName { get; set; } = "Lantern";
    public int ProtocolVersion { get; set; } = 800;
    public string ServerVersion { get; set; } = "1.20.80";
    public int OnlinePlayers { get; set; } = 0;
    public int MaxPlayers { get; set; } = 20;
    public long ServerGuid { get; set; }
    public string Motd { get; set; } = "A Minecraft server powered by Lantern";
    public string GameMode { get; set; } = "Survival";
    public int GameModeId { get; set; } = 1;
    public int PortIpv4 { get; set; } = 19132;
    public int PortIpv6 { get; set; } = 19133;

    public override string ToString() {
        return $"{Edition};{ServerName};{ProtocolVersion};{ServerVersion};{OnlinePlayers};{MaxPlayers};{ServerGuid};{Motd};{GameMode};{GameModeId};{PortIpv4};{PortIpv6};";
    }
}