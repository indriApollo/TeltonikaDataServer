namespace TeltonikaDataServer.Config;

public class ServerOptions
{
    public const string Server = "Server";

    public string Ip { get; set; } = "0.0.0.0";
    public ushort Port { get; set; } = 8160;
    public long PacketIdCacheSize { get; set; } = 1000;
}
