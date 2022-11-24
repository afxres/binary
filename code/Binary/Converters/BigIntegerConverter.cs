namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Numerics;

internal sealed class BigIntegerConverter : VariableWriterEncodeConverter<BigInteger, BigIntegerConverter.Functions>
{
    internal struct Functions : IVariableWriterEncodeConverterFunctions<BigInteger>
    {
        public static int GetMaxLength(BigInteger item)
        {
            return item.GetByteCount();
        }

        public static int Encode(Span<byte> span, BigInteger item)
        {
            if (item.TryWriteBytes(span, out var actual) is false)
                ThrowHelper.ThrowTryWriteBytesFailed();
            return actual;
        }

        public static BigInteger Decode(in ReadOnlySpan<byte> span)
        {
            return new BigInteger(span);
        }
    }
}
