using TeltonikaDataServer.Models;

namespace TeltonikaDataServer;

public class TeltonikaDataHandler
{
    private readonly ILogger<TeltonikaDataHandler> _logger;
    private readonly TelematicsDataRepository _repository;

    public TeltonikaDataHandler(ILogger<TeltonikaDataHandler> logger, TelematicsDataRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Handle(Codec8UdpPacket parsedPacket)
    {
        string imei = System.Text.Encoding.ASCII.GetString(parsedPacket.AvlHeader.Imei);
        _logger.LogDebug($"imei {imei}");

        int? devId = await _repository.GetDeviceIdFromCache(imei);
        _logger.LogDebug($"devid {devId}");
        if (devId == null)
        {
            _logger.LogWarning($"Ignoring unknown IMEI {imei}");
            return;
        }

        _logger.LogDebug($"avl count {parsedPacket.AvlHeader.AvlDataElementsCount}");
        foreach (var avl in parsedPacket.AvlDatas)
        {
            GpsPositionIn pos = new()
            {
                Latitude = avl.Latitude,
                Longitude = avl.Longitude,
                Altitude = (short)avl.Altitude,
                Heading = (short)avl.Angle,
                Speed = (short)avl.Speed,
                TimestampUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)avl.Timestamp).UtcDateTime,
                DevId = devId.Value
            };

            _logger.LogDebug($"store pos {pos.Latitude} {pos.Longitude} {pos.TimestampUtc}");
            await _repository.StoreGpsPosition(pos);
        }
    }
}
