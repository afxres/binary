namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Numerics;

internal sealed class BigIntegerConverter : VariablePrefixEncodeConverter<BigInteger, BigIntegerConverter.Functions>
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

    internal struct Functions : IVariablePrefixEncodeConverterFunctions<BigInteger>
    {
        public static BigInteger Decode(in ReadOnlySpan<byte> span)
        {
            return DecodeInternal(span);
        }

        public static void Encode(ref Allocator allocator, BigInteger item)
        {
            Allocator.Append(ref allocator, item.GetByteCount(), item, EncodeFunction);
        }

        public static void EncodeWithLengthPrefix(ref Allocator allocator, BigInteger item)
        {
            Allocator.AppendWithLengthPrefix(ref allocator, item.GetByteCount(), item, EncodeFunction);
        }
    }
}
