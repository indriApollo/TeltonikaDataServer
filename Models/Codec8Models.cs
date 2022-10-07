namespace TeltonikaDataServer.Models;

public static class Codec8Constants
{
    public const int IMEI_LENGTH = 15; // imei is always 15 chars
    public const ushort ACK_LENGTH = 5; // ack is always 5 bytes (ignoring first two packet length bytes)
}

public record class Codec8UdpHeader
{
    public ushort PacketLength { get; init; }
    public ushort PacketId { get; init; }
    public byte UnusedByte { get; init; } // normally 0x01
}

public record class Codec8UdpAck : Codec8UdpHeader
{
    public byte AvlPacketId { get; init; }
    public byte AcceptedAvlDataElementsCount { get; init; }
}

public record class Codec8UdpAvlHeader
{
    public byte AvlPacketId { get; init; }
    public ushort ImeiLength { get; init; } // should always be 0x000F
    public byte[] Imei { get; init; } = null!;
    public byte AvlDataElementsCount { get; init; }
}

public record class Codec8UdpAvlData
{
    public ulong Timestamp { get; init; }
    public byte Priority { get; init; }
    public int Longitude { get; init; }
    public int Latitude { get; init; }
    public ushort Altitude { get; init; }
    public ushort Angle { get; init; }
    public byte SatellitesCount { get; init; }
    public ushort Speed { get; init; }
    public byte EventIoId { get; init; }
    public byte IoTotalElementsCount { get; init; }
    public byte SingleByteIoElementsCount { get; init; }
    public List<Codec8UdpIoElement<byte>> SingleByteIoElements { get; init; } = new();
    public byte TwoBytesIoElementsCount { get; init; }
    public List<Codec8UdpIoElement<ushort>> TwoBytesIoElements { get; init; } = new();
    public byte FourBytesIoElementsCount { get; init; }
    public List<Codec8UdpIoElement<uint>> FourBytesIoElements { get; init; } = new();
    public byte EightBytesIoElementsCount { get; init; }
    public List<Codec8UdpIoElement<ulong>> EightBytesIoElements { get; init; } = new();
}

public record class Codec8UdpIoElement<T> where T : unmanaged
{
    public byte IoId { get; init; }
    public T IoValue { get; init; }
}

public record class Codec8UdpPacket
{
    public Codec8UdpHeader Header { get; init; } = null!;
    public Codec8UdpAvlHeader AvlHeader { get; init; } = null!;
    public List<Codec8UdpAvlData> AvlDatas { get; init; } = null!;
}
