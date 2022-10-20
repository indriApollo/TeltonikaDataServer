using System.Net;
using System.Net.Sockets;
using TeltonikaDataServer.Config;
using Microsoft.Extensions.Options;

namespace TeltonikaDataServer;

public class UdpServer : BackgroundService
{
    private readonly ILogger<UdpServer> _logger;
    private readonly ServerOptions _config;
    private readonly TeltonikaDataHandler _dataHandler;

    public UdpServer(ILogger<UdpServer> logger, IOptions<ServerOptions> options, TeltonikaDataHandler dataHandler)
    {
        _logger = logger;
        _config = options.Value;
        _dataHandler = dataHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var packetIdCache = new PacketIdCache(_config.PacketIdCacheSize);

        _logger.LogInformation($"Listening on UDP {_config.Ip}:{_config.Port}");
        UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(_config.Ip), _config.Port));

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await udpClient.ReceiveAsync(stoppingToken);
            _logger.LogDebug($"Got {result.Buffer.Length} bytes from {result.RemoteEndPoint}");
            var packet = Codec8Parser.Parse(result.Buffer);

            byte? latestAvlPacketId = packetIdCache.GetLatestPacketId(packet.AvlHeader.Imei);
            // skip processing if duplicate packet (UDP acks can get lost)
            if (latestAvlPacketId.HasValue && latestAvlPacketId == packet.AvlHeader.AvlPacketId)
            {
                _logger.LogWarning($"Skipping duplicate AVL packet id {packet.AvlHeader.AvlPacketId}");
            }
            else
            {
                // XXX
                _ = Task.Run(async () => {
                    try
                    {
                        await _dataHandler.Handle(packet);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "handler exception");
                    }
                });
            }

            var ack = Codec8Parser.BuildAck(packet);
            var ackPayload = Codec8Parser.SerializeAck(ack);
            await udpClient.SendAsync(ackPayload, ackPayload.Length, result.RemoteEndPoint);
            packetIdCache.SaveLatestPacketId(packet.AvlHeader.Imei, packet.AvlHeader.AvlPacketId);
        }
    }
}
