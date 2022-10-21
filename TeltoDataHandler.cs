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
            DateTime timestampUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)avl.Timestamp).UtcDateTime;
            await HandleGpsPosition(avl, devId.Value, timestampUtc);
            await HandleIoElements(avl, devId.Value, timestampUtc);
        }
    }

    private async Task HandleGpsPosition(Codec8UdpAvlData avl, int devId, DateTime timestampUtc)
    {
        GpsPositionIn pos = new()
        {
            Latitude = avl.Latitude,
            Longitude = avl.Longitude,
            Altitude = (short)avl.Altitude,
            Heading = (short)avl.Angle,
            Speed = (short)avl.Speed,
            TimestampUtc = timestampUtc,
            DevId = devId
        };

        _logger.LogDebug($"store pos {pos.Latitude} {pos.Longitude} {pos.TimestampUtc}");
        await _repository.StoreGpsPosition(pos);
    }

    private async Task HandleIoElements(Codec8UdpAvlData avl, int devId, DateTime timestampUtc)
    {
        _logger.LogDebug($"Io event id {avl.EventIoId} count {avl.IoTotalElementsCount}");
        if (avl.IoTotalElementsCount <= 0)
            return;

        HandleSingleByteIoElements(avl, devId, timestampUtc);
        await HandleTwoBytesIoElements(avl, devId, timestampUtc);
        HandleFourBytesIoElements(avl, devId, timestampUtc);
        HandleEightBytesIoElements(avl, devId, timestampUtc);
    }

    private void HandleSingleByteIoElements(Codec8UdpAvlData avl, int devId, DateTime timestampUtc)
    {
        foreach (var ioElement in avl.SingleByteIoElements)
        {
            _logger.LogDebug($"single byte io {ioElement.IoId} {ioElement.IoValue}");
        }
    }

    private async Task HandleTwoBytesIoElements(Codec8UdpAvlData avl, int devId, DateTime timestampUtc)
    {
        foreach (var ioElement in avl.TwoBytesIoElements)
        {
            _logger.LogDebug($"two bytes io {ioElement.IoId} {ioElement.IoValue}");
            if (ioElement.IoId == (byte)FmbIoElementId.EXTERNAL_VOLTAGE)
                await HandleExternalVoltage(ioElement.IoValue, devId, timestampUtc);
        }
    }

    private void HandleFourBytesIoElements(Codec8UdpAvlData avl, int devId, DateTime timestampUtc)
    {
        foreach (var ioElement in avl.FourBytesIoElements)
        {
            _logger.LogDebug($"four bytes io {ioElement.IoId} {ioElement.IoValue}");
        }
    }

    private void HandleEightBytesIoElements(Codec8UdpAvlData avl, int devId, DateTime timestampUtc)
    {
        foreach (var ioElement in avl.EightBytesIoElements)
        {
            _logger.LogDebug($"eight bytes io {ioElement.IoId} {ioElement.IoValue}");
        }
    }

    private async Task HandleExternalVoltage(ushort value, int devId, DateTime timestampUtc)
    {
        ExternalVoltageIn exv = new()
        {
            Value = (short)(value/10), // transform from device precision 0.001 to db column precision 0.01
            TimestampUtc = timestampUtc,
            DevId = devId
        };

        _logger.LogDebug($"store exv {exv.Value} {exv.TimestampUtc}");
        await _repository.StoreExternalVoltage(exv);
    }
}
