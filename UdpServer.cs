using System.Net;
using System.Net.Sockets;
using TeltonikaDataServer.Config;
using TeltonikaDataServer.Models;
using Microsoft.Extensions.Options;

namespace TeltonikaDataServer;

public class UdpServer : BackgroundService
{
    private readonly ILogger<UdpServer> _logger;
    private readonly ServerOptions _config;
    private readonly TeltonikaDataHandler _dataHandler;
    private readonly List<Task> _handlerTasks = new();

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

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                _logger.LogDebug($"Got {result.Buffer.Length} bytes from {result.RemoteEndPoint}");
                var packet = Codec8Parser.Parse(result.Buffer);

                byte? latestAvlPacketId = packetIdCache.GetLatestPacketId(packet.AvlHeader.Imei);
                _logger.LogDebug($"PacketId {packet.Header.PacketId} AvlPacketId {packet.AvlHeader.AvlPacketId}");
                // skip processing if duplicate packet (UDP acks can get lost)
                if (latestAvlPacketId.HasValue && latestAvlPacketId == packet.AvlHeader.AvlPacketId)
                {
                    _logger.LogWarning($"Skipping duplicate AVL packet id {packet.AvlHeader.AvlPacketId}");
                }
                else
                {
                    HandlePacketInBackground(packet);
                }

                // always ack quickly to lower gsm power consumption on battery powered trackers
                // in case of handling exception, data will be lost atm ¯\_(ツ)_/¯
                var ack = Codec8Parser.BuildAck(packet);
                var ackPayload = Codec8Parser.SerializeAck(ack);
                await udpClient.SendAsync(ackPayload, ackPayload.Length, result.RemoteEndPoint);
                packetIdCache.SaveLatestPacketId(packet.AvlHeader.Imei, packet.AvlHeader.AvlPacketId);
            }
        }
        finally
        {
            RemoveCompletedHandlerTasks();
            _logger.LogInformation($"Awaiting {_handlerTasks.Count} handler tasks");
            await Task.WhenAll(_handlerTasks);
        }
    }

    private void RemoveCompletedHandlerTasks() => _handlerTasks.RemoveAll(t => t.IsCompleted);

    private void HandlePacketInBackground(Codec8UdpPacket parsedPacket)
    {
        RemoveCompletedHandlerTasks();
        _handlerTasks.Add(Handlepacket(parsedPacket));
    }

    private async Task Handlepacket(Codec8UdpPacket parsedPacket)
    {
        try
        {
            await _dataHandler.Handle(parsedPacket);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Handler exception");
        }
    }
}
