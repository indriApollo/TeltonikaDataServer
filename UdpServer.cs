using System.Net;
using System.Net.Sockets;

namespace TeltonikaDataServer;

public class UdpServer : BackgroundService
{
    private readonly ILogger<UdpServer> _logger;
    private readonly IConfiguration _config;

    public UdpServer(ILogger<UdpServer> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string ip = _config.GetValue<string?>("Ip") ?? "0.0.0.0";
        ushort port = _config.GetValue<ushort?>("Port") ?? 8160;

        _logger.LogInformation($"Listening on UDP {ip}:{port}");
        UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), port));

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await udpClient.ReceiveAsync(stoppingToken);
            var packet = Codec8Parser.Parse(result.Buffer);
            //
            var ack = Codec8Parser.BuildAck(packet);
            await udpClient.SendAsync(Codec8Parser.SerializeAck(ack), ack.PacketLength, result.RemoteEndPoint);
        }
    }
}
