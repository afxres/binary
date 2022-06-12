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
        var target = MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, length), length);
        var flag = item.TryWriteBytes(target, out var actual);
        Debug.Assert(flag);
        Debug.Assert(actual == length);
    }

    public override BigInteger Decode(in ReadOnlySpan<byte> span)
    {
        return new BigInteger(span);
    }
}
