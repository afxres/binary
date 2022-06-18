namespace Mikodev.Binary.Converters;

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

internal sealed class BigIntegerConverter : Converter<BigInteger>
{
    public override void Encode(ref Allocator allocator, BigInteger item)
    {
        var length = item.GetByteCount();
        var buffer = MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, length), length);
        var result = item.TryWriteBytes(buffer, out var actual);
        Debug.Assert(result);
        Debug.Assert(actual == length);
    }

    public override BigInteger Decode(in ReadOnlySpan<byte> span)
    {
        return new BigInteger(span);
    }
}
