namespace Mikodev.Binary;

using Mikodev.Binary.Internal.Metadata;
using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

public static partial class ConverterExtensions
{
    private static T DecodeBrotliInternal<T>(DecodeReadOnlyDelegate<T> decode, ReadOnlySpan<byte> source, ArrayPool<byte> arrays)
    {
        var limits = Math.Max(64 * 1024, checked(source.Length * 2));
        var memory = arrays.Rent(limits);
        var handle = new BrotliDecoder();
        var offset = 0;
        var length = 0;

        try
        {
            while (true)
            {
                limits = Math.Max(limits, memory.Length);
                var status = handle.Decompress(source.Slice(offset), new Span<byte>(memory, length, limits - length), out var bytesConsumed, out var bytesWritten);
                offset += bytesConsumed;
                length += bytesWritten;

                var intent = new ReadOnlySpan<byte>(memory, 0, length);
                if (status is OperationStatus.Done)
                    return decode.Invoke(intent);
                if (status is not OperationStatus.DestinationTooSmall)
                    throw new IOException($"Brotli decode failed, status: {status}");

                limits = checked(limits * 2);
                var buffer = arrays.Rent(limits);
                intent.CopyTo(new Span<byte>(buffer));
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

    public static object? DecodeBrotli(this IConverter converter, scoped ReadOnlySpan<byte> span)
    {
        return DecodeBrotliInternal(converter.Decode, span, ArrayPool<byte>.Shared);
    }

    public static T DecodeBrotli<T>(this Converter<T> converter, scoped ReadOnlySpan<byte> span)
    {
        return DecodeBrotliInternal(converter.Decode, span, ArrayPool<byte>.Shared);
    }
}
