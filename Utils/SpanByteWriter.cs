using System.Buffers.Binary;

namespace TeltonikaDataServer.Utils;

public ref struct SpanByteWriter
{
    private int index;
    private readonly Span<byte> span;

    public SpanByteWriter(byte[] buffer)
    {
        index = 0;
        span = new Span<byte>(buffer);
    }

    public void WriteByte(byte value)
    {
        span[index++] = value;
    }

    public void WriteUInt16(ushort value)
    {
        const int typeSize = sizeof(ushort);
        BinaryPrimitives.WriteUInt16BigEndian(span.Slice(index, typeSize), value);
        index += typeSize;
    }
}
