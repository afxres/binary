namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Metadata;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static partial class Converter
{
    public static Type GetGenericArgument(IConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        var metadata = converter as IConverterMetadata;
        if (metadata is null)
            ThrowHelper.ThrowNotConverter(converter.GetType());
        return metadata.GetGenericArgument();
    }

    public static MethodInfo GetMethod(IConverter converter, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(converter);
        var metadata = converter as IConverterMetadata;
        if (metadata is null)
            ThrowHelper.ThrowNotConverter(converter.GetType());
        return metadata.GetMethod(name);
    }

    public static void Encode(scoped Span<byte> span, int number, out int bytesWritten)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(number);
        var numberLength = NumberModule.EncodeLength((uint)number);
        if (span.Length < numberLength)
            ThrowHelper.ThrowNotEnoughBytesToWrite();
        NumberModule.Encode(ref MemoryMarshal.GetReference(span), (uint)number, numberLength);
        bytesWritten = numberLength;
    }

    public static int Decode(scoped ReadOnlySpan<byte> span, out int bytesRead)
    {
        ref var source = ref MemoryMarshal.GetReference(span);
        var limits = span.Length;
        var offset = 0;
        var length = NumberModule.Decode(ref source, ref offset, limits);
        bytesRead = offset;
        return length;
    }

    public static void Encode(ref Allocator allocator, int number)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(number);
        var numberLength = NumberModule.EncodeLength((uint)number);
        NumberModule.Encode(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
    }

    public static int Decode(ref ReadOnlySpan<byte> span)
    {
        ref var source = ref MemoryMarshal.GetReference(span);
        var limits = span.Length;
        var offset = 0;
        var length = NumberModule.Decode(ref source, ref offset, limits);
        span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, offset), limits - offset);
        return length;
    }

    public static void EncodeWithLengthPrefix(ref Allocator allocator, scoped ReadOnlySpan<byte> span)
    {
        var length = span.Length;
        var numberLength = NumberModule.EncodeLength((uint)length);
        ref var target = ref Allocator.Assign(ref allocator, length + numberLength);
        NumberModule.Encode(ref target, (uint)length, numberLength);
        if (length is 0)
            return;
        Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref target, numberLength), ref MemoryMarshal.GetReference(span), (uint)length);
    }

    public static ReadOnlySpan<byte> DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
    {
        ref var source = ref MemoryMarshal.GetReference(span);
        var limits = span.Length;
        var offset = 0;
        var length = NumberModule.DecodeEnsureBuffer(ref source, ref offset, limits);
        var cursor = offset + length;
        span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, cursor), limits - cursor);
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, offset), length);
    }
}
