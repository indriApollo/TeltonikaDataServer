using TeltonikaDataServer.Models;
using System.Buffers.Binary;

namespace TeltonikaDataServer;

public static class Codec8Parser
{
    public static Codec8UdpPacket Parse(byte[] buffer)
    {
        var reader = new ReadOnlySpanByteReader(buffer);

        var header = ParseHeader(ref reader);
        var avlHeader = ParseAvlHeader(ref reader);
        if (avlHeader.CodecId != 0x08) // XXX
            throw new Exception("codec id is not 8");

        var avlDatas = ParseAvlDatas(ref reader, avlHeader.AvlDataElementsCount);

        return new()
        {
            Header = header,
            AvlHeader = avlHeader,
            AvlDatas = avlDatas
        };
    }

    public static Codec8UdpAck BuildAck(Codec8UdpPacket packet)
    {
        return new()
        {
            PacketLength = Codec8Constants.ACK_LENGTH,
            PacketId = packet.Header.PacketId,
            UnusedByte = 0,
            AvlPacketId = packet.AvlHeader.AvlPacketId,
            AcceptedAvlDataElementsCount = packet.AvlHeader.AvlDataElementsCount
        };
    }

    public static byte[] SerializeAck(Codec8UdpAck ack)
    {
        var stream = new MemoryStream(Codec8Constants.ACK_PACKET_SIZE);

        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(ack.PacketLength);
            writer.Write(ack.PacketId);
            writer.Write(ack.UnusedByte);
            writer.Write(ack.AvlPacketId);
            writer.Write(ack.AcceptedAvlDataElementsCount);
        }

        return stream.GetBuffer();
    }

    private static Codec8UdpHeader ParseHeader(ref ReadOnlySpanByteReader reader)
    {
        return new()
        {
            PacketLength = reader.ReadUInt16(),
            PacketId = reader.ReadUInt16(),
            UnusedByte = reader.ReadByte()
        };
    }

    private static Codec8UdpAvlHeader ParseAvlHeader(ref ReadOnlySpanByteReader reader)
    {
        return new()
        {
            AvlPacketId = reader.ReadByte(),
            ImeiLength = reader.ReadUInt16(),
            Imei = reader.ReadByteArray(Codec8Constants.IMEI_LENGTH),
            CodecId = reader.ReadByte(),
            AvlDataElementsCount = reader.ReadByte()
        };
    }

    private static List<Codec8UdpAvlData> ParseAvlDatas(ref ReadOnlySpanByteReader reader, byte avlDataElementsCount)
    {
        List<Codec8UdpAvlData> avlDatas = new();

        for (byte i = 0; i < avlDataElementsCount; i++)
        {
            avlDatas.Add(ParseAvlData(ref reader));
        }

        return avlDatas;
    }

    private delegate T ReadValue<T>(ref ReadOnlySpanByteReader reader);
    private static Codec8UdpAvlData ParseAvlData(ref ReadOnlySpanByteReader reader)
    {
        ulong timestamp = reader.ReadUint64();
        byte priority = reader.ReadByte();

        int longitude = reader.ReadInt32();
        int latitude = reader.ReadInt32();
        ushort altitude = reader.ReadUInt16();
        ushort angle = reader.ReadUInt16();
        byte satellitesCount = reader.ReadByte();
        ushort speed = reader.ReadUInt16();

        byte eventIoId = reader.ReadByte();
        byte ioTotalElementsCount = reader.ReadByte();

        byte singleByteIoElementsCount = reader.ReadByte();
        List<Codec8UdpIoElement<byte>> singleByteIoElements = ParseIoElements<byte>(ref reader, (ref ReadOnlySpanByteReader reader) => reader.ReadByte(), singleByteIoElementsCount);

        byte twoBytesIoElementsCount = reader.ReadByte();
        List<Codec8UdpIoElement<ushort>> twoBytesIoElements = ParseIoElements<ushort>(ref reader, (ref ReadOnlySpanByteReader reader) => reader.ReadUInt16(), twoBytesIoElementsCount);

        byte fourBytesIoElementsCount = reader.ReadByte();
        List<Codec8UdpIoElement<uint>> fourBytesIoElements = ParseIoElements<uint>(ref reader, (ref ReadOnlySpanByteReader reader) => reader.ReadUInt32(), fourBytesIoElementsCount);

        byte eightBytesIoElementsCount = reader.ReadByte();
        List<Codec8UdpIoElement<ulong>> eightBytesIoElements = ParseIoElements<ulong>(ref reader, (ref ReadOnlySpanByteReader reader) => reader.ReadUint64(), eightBytesIoElementsCount);

        return new()
        {
            Timestamp = timestamp,
            Priority = priority,
            Longitude = longitude,
            Latitude = latitude,
            Altitude = altitude,
            Angle = angle,
            SatellitesCount = satellitesCount,
            Speed = speed,
            EventIoId = eventIoId,
            IoTotalElementsCount = ioTotalElementsCount,
            SingleByteIoElementsCount = singleByteIoElementsCount,
            SingleByteIoElements = singleByteIoElements,
            TwoBytesIoElementsCount = twoBytesIoElementsCount,
            TwoBytesIoElements = twoBytesIoElements,
            FourBytesIoElementsCount = fourBytesIoElementsCount,
            FourBytesIoElements = fourBytesIoElements,
            EightBytesIoElementsCount = eightBytesIoElementsCount,
            EightBytesIoElements = eightBytesIoElements
        };
    }

    private static List<Codec8UdpIoElement<T>> ParseIoElements<T>(ref ReadOnlySpanByteReader reader, ReadValue<T> readValue, byte count) where T : unmanaged
    {
        List<Codec8UdpIoElement<T>> ioElements = new();

        for (byte i = 0; i < count; i++)
        {
            Codec8UdpIoElement<T> ioElement = new()
            {
                IoId = reader.ReadByte(),
                IoValue = readValue(ref reader)
            };
            ioElements.Add(ioElement);
        }

        return ioElements;
    }

    private ref struct ReadOnlySpanByteReader
    {
        private int index;
        private readonly ReadOnlySpan<byte> span;

        public ReadOnlySpanByteReader(byte[] buffer)
        {
            index = 0;
            span = new ReadOnlySpan<byte>(buffer);
        }

        public byte ReadByte()
        {
            byte result = span[index];
            index++;
            return result;
        }

        public byte[] ReadByteArray(int arrayLength)
        {
            byte[] result = span.Slice(index, arrayLength).ToArray();
            index += arrayLength;
            return result;
        }

        public ushort ReadUInt16()
        {
            const int typeSize = sizeof(ushort);
            ushort result = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(index, typeSize));
            index += typeSize;
            return result;
        }

        public int ReadInt32()
        {
            const int typeSize = sizeof(int);
            int result = BinaryPrimitives.ReadInt32BigEndian(span.Slice(index, typeSize));
            index += typeSize;
            return result;
        }

        public uint ReadUInt32()
        {
            const int typeSize = sizeof(uint);
            uint result = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(index, typeSize));
            index += typeSize;
            return result;
        }

        public ulong ReadUint64()
        {
            const int typeSize = sizeof(ulong);
            ulong result = BinaryPrimitives.ReadUInt64BigEndian(span.Slice(index, typeSize));
            index += typeSize;
            return result;
        }
    }
}
