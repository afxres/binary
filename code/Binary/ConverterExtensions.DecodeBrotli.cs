namespace Mikodev.Binary;

using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

public static partial class ConverterExtensions
{
    private static T DecodeBrotliInternal<T>(Converter<T> converter, ReadOnlySpan<byte> source, ArrayPool<byte> arrays)
    {
        var memory = arrays.Rent(source.Length * 2);
        var handle = new BrotliDecoder();

        try
        {
            var offset = 0;
            var length = 0;

            while (true)
            {
                var status = handle.Decompress(source.Slice(offset), new Span<byte>(memory, length, memory.Length - length), out var bytesConsumed, out var bytesWritten);
                offset += bytesConsumed;
                length += bytesWritten;

                if (status is OperationStatus.Done)
                    return converter.Decode(new ReadOnlySpan<byte>(memory, 0, length));
                if (status is not OperationStatus.DestinationTooSmall)
                    throw new IOException($"Brotli decode error, status: {status}");

                var buffer = arrays.Rent(memory.Length * 2);
                new ReadOnlySpan<byte>(memory, 0, length).CopyTo(new Span<byte>(buffer));
                arrays.Return(memory);
                memory = buffer;
            }
        }
        finally
        {
            handle.Dispose();
            arrays.Return(memory);
        }
    }

    public static T DecodeBrotli<T>(this Converter<T> converter, scoped ReadOnlySpan<byte> span)
    {
        return DecodeBrotliInternal(converter, span, ArrayPool<byte>.Shared);
    }
}
