namespace Mikodev.Binary.Creators.Isolated.Variables;

using Mikodev.Binary;
using Mikodev.Binary.Internal;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class BitArrayConverter : Converter<BitArray?>
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_array")]
    private static extern ref byte[]? AccessFunction(BitArray array);

    private static void Transform(Span<byte> target, ReadOnlySpan<byte> source, int length)
    {
        var bounds = length >> 3;
        source.Slice(0, bounds).CopyTo(target);
        var remain = length & 7;
        if (remain is 0)
            return;
        target[bounds] = (byte)(source[bounds] & ((1 << remain) - 1));
    }

    public override void Encode(ref Allocator allocator, BitArray? item)
    {
        if (item is null)
            return;
        var length = item.Count;
        var margin = (-length) & 7;
        Debug.Assert((uint)margin < 8);
        Converter.Encode(ref allocator, margin);
        if (length is 0)
            return;
        var required = (int)(((uint)length + 7U) >> 3);
        var buffer = MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, required), required);
        var source = AccessFunction(item);
        Transform(buffer, source, length);
    }

    public override BitArray? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        var cursor = span;
        var margin = Converter.Decode(ref cursor);
        Debug.Assert(margin >= 0);
        if (margin is 0 && cursor.Length is 0)
            return new BitArray(0);
        if (margin >= 8 || cursor.Length is 0)
            ThrowHelper.ThrowNotEnoughBytes();
        var length = checked((int)(((ulong)cursor.Length << 3) - (uint)margin));
        var result = new BitArray(length);
        var target = AccessFunction(result);
        Transform(target, cursor, length);
        return result;
    }
}
