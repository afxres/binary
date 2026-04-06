namespace Mikodev.Binary;

using Mikodev.Binary.Internal.Contexts;
using Mikodev.Binary.Internal.Metadata;
using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

public static partial class ConverterExtensions
{
    private static T DecodeBrotliInternal<T>(DecodeReadOnlyDelegate<T> decode, ReadOnlySpan<byte> source, ArrayPool<byte> arrays)
    {
        var bounds = Math.Max(64 * 1024, checked(source.Length * 2));
        var memory = arrays.Rent(bounds);
        var offset = 0;
        var length = 0;

        using var handle = new BrotliDecoder();
        using var _ = new ArrayPoolReturnHelper<byte>(arrays, ref memory);
        while (true)
        {
            bounds = Math.Max(bounds, memory.Length);
            var status = handle.Decompress(source.Slice(offset), new Span<byte>(memory, length, bounds - length), out var bytesConsumed, out var bytesWritten);
            offset += bytesConsumed;
            length += bytesWritten;

            var intent = new ReadOnlySpan<byte>(memory, 0, length);
            if (status is OperationStatus.Done)
                return decode.Invoke(intent);
            if (status is not OperationStatus.DestinationTooSmall)
                throw new IOException($"Brotli decode failed, status: {status}");

            bounds = checked(bounds * 2);
            var buffer = arrays.Rent(bounds);
            intent.CopyTo(new Span<byte>(buffer));
            arrays.Return(memory);
            memory = buffer;
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
