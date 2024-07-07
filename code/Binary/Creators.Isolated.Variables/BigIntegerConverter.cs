namespace Mikodev.Binary.Creators.Isolated.Variables;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Numerics;

internal sealed class BigIntegerConverter : VariableConverter<BigInteger, BigIntegerConverter.Functions>
{
    internal readonly struct Functions : IVariableConverterFunctions<BigInteger>
    {
        public static int Limits(BigInteger item)
        {
            return item.GetByteCount();
        }

        public static int Append(Span<byte> span, BigInteger item)
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
