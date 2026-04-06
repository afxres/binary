namespace Mikodev.Binary;

using Mikodev.Binary.Internal.Contexts;
using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

public static partial class ConverterExtensions
{
    private static void EncodeBrotliInternal(ReadOnlySpan<byte> source, Span<byte> target, out int bytesWritten)
    {
        if (BrotliEncoder.TryCompress(source, target, out bytesWritten, quality: 1, window: 22))
            return;
        throw new IOException("Brotli encode failed.");
    }

    private static byte[] EncodeBrotliInternal(ReadOnlySpan<byte> source, ArrayPool<byte> arrays)
    {
        var length = BrotliEncoder.GetMaxCompressedLength(source.Length);
        var memory = arrays.Rent(length);

        using var _ = new ArrayPoolReturnHelper<byte>(arrays, ref memory);
        var target = new Span<byte>(memory);
        EncodeBrotliInternal(source, target, out var bytesWritten);
        return target.Slice(0, bytesWritten).ToArray();
    }

    private static byte[] EncodeBrotliInternal<T>(AllocatorAction<T> action, T item, ArrayPool<byte> arrays)
    {
        using var underlying = new ArrayPoolAllocator(arrays);
        var allocator = new Allocator(underlying);
        action.Invoke(ref allocator, item);
        return EncodeBrotliInternal(allocator.AsSpan(), arrays);
    }

    public static byte[] EncodeBrotli(this IConverter converter, object? item)
    {
        return EncodeBrotliInternal(converter.Encode, item, ArrayPool<byte>.Shared);
    }

    public static byte[] EncodeBrotli<T>(this Converter<T> converter, T item)
    {
        return EncodeBrotliInternal(converter.Encode, item, ArrayPool<byte>.Shared);
    }
}
