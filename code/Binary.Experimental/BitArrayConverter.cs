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
        static BitArray Except() => throw new ArgumentException($"Invalid header or not enough bytes, type: {typeof(BitArray)}");

        if (span.Length is 0)
            return null;
        var intent = span;
        var header = (uint)Converter.Decode(ref intent);
        if (header > 7 || (intent.Length is 0 && header is not 0))
            return Except();
        var buffer = intent.ToArray();
        var result = new BitArray(buffer);
        result.Length -= (int)header;
        return result;
    }
}
