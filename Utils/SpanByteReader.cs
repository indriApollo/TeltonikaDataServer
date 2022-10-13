using System.Buffers.Binary;

namespace TeltonikaDataServer.Utils;

public ref struct SpanByteReader
{
    private int index;
    private readonly Span<byte> span;

    public SpanByteReader(byte[] buffer)
    {
        index = 0;
        span = new Span<byte>(buffer);
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
