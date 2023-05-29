namespace Mikodev.Binary;

using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public static partial class Converter
{
    public static void Encode(Stream stream, int number)
    {
        var buffer = (stackalloc byte[4]);
        Encode(buffer, number, out var length);
        stream.Write(buffer.Slice(0, length));
    }

    public static async ValueTask EncodeAsync(Stream stream, int number, CancellationToken cancellation = default)
    {
        var buffer = new byte[4];
        Encode(new Span<byte>(buffer), number, out var length);
        await stream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, length), cancellation).ConfigureAwait(false);
    }

    public static int Decode(Stream stream)
    {
        var buffer = (stackalloc byte[4]);
        stream.ReadExactly(buffer.Slice(0, 1));
        var header = (uint)buffer[0];
        if ((header & 0x80U) is 0)
            return (int)header;
        stream.ReadExactly(buffer.Slice(1, 3));
        var result = BinaryPrimitives.ReadUInt32BigEndian(buffer);
        return (int)(result & 0x7FFF_FFFFU);
    }

    public static async ValueTask<int> DecodeAsync(Stream stream, CancellationToken cancellation = default)
    {
        var buffer = new byte[4];
        await stream.ReadExactlyAsync(new Memory<byte>(buffer, 0, 1), cancellation).ConfigureAwait(false);
        var header = (uint)buffer[0];
        if ((header & 0x80U) is 0)
            return (int)header;
        await stream.ReadExactlyAsync(new Memory<byte>(buffer, 1, 3), cancellation).ConfigureAwait(false);
        var result = BinaryPrimitives.ReadUInt32BigEndian(buffer);
        return (int)(result & 0x7FFF_FFFFU);
    }
}
