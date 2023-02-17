namespace Mikodev.Binary.Experimental;

using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public sealed class BitArrayConverter : Converter<BitArray?>
{
    /* BitArray Converter
     * Layout: padding bit count for last byte | bytes
     */

    public override void Encode(ref Allocator allocator, BitArray? item)
    {
        if (item is null)
            return;
        var bitLength = item.Length;
        var padding = (-bitLength) & 7;
        Converter.Encode(ref allocator, padding);
        if (bitLength is 0)
            return;
        var byteLength = (bitLength + 7) / 8;
        var bytes = new byte[byteLength];
        item.CopyTo(bytes, 0);
        Allocator.Append(ref allocator, bytes);
    }

    public override BitArray? Decode(in ReadOnlySpan<byte> span)
    {
        [DoesNotReturn]
        [DebuggerStepThrough]
        static BitArray Except() => throw new ArgumentException($"Invalid padding byte(s), type: {typeof(BitArray)}");

        var limits = span.Length;
        if (limits is 0)
            return null;
        var intent = span;
        var header = (uint)Converter.Decode(ref intent);
        if (intent.IsEmpty)
            return header is 0 ? new BitArray(0) : Except();
        if (header >= 8)
            return Except();
        var buffer = intent.ToArray();
        var result = new BitArray(buffer);
        result.Length -= (int)header;
        return result;
    }
}
