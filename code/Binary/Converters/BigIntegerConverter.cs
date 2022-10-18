namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;
using System.Numerics;

internal sealed class BigIntegerConverter : Converter<BigInteger>
{
    private static readonly AllocatorWriter<BigInteger> EncodeFunction;

    static BigIntegerConverter()
    {
        static int Invoke(Span<byte> span, BigInteger item)
        {
            if (item.TryWriteBytes(span, out var actual) is false)
                ThrowHelper.ThrowTryWriteBytesFailed();
            return actual;
        }
        EncodeFunction = Invoke;
    }

    private static BigInteger DecodeInternal(ReadOnlySpan<byte> span)
    {
        return new BigInteger(span);
    }

    public override void Encode(ref Allocator allocator, BigInteger item) => Allocator.Append(ref allocator, item.GetByteCount(), item, EncodeFunction);

    public override void EncodeAuto(ref Allocator allocator, BigInteger item) => Allocator.AppendWithLengthPrefix(ref allocator, item.GetByteCount(), item, EncodeFunction);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, BigInteger item) => Allocator.AppendWithLengthPrefix(ref allocator, item.GetByteCount(), item, EncodeFunction);

    public override BigInteger Decode(in ReadOnlySpan<byte> span) => DecodeInternal(span);

    public override BigInteger DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));

    public override BigInteger DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));
}
