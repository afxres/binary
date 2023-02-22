namespace Mikodev.Binary.Internal.Sequence;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal static class SequenceContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetCapacityOrDefault<E>(int byteLength, int itemLength)
    {
        Debug.Assert(byteLength > 0);
        Debug.Assert(itemLength >= 0);
        const int FallbackCapacity = 8;
        return itemLength > 0 ? GetCapacity<E>(byteLength, itemLength) : FallbackCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetCapacity<E>(int byteLength, int itemLength)
    {
        Debug.Assert(byteLength > 0);
        Debug.Assert(itemLength > 0);
        var quotient = Math.DivRem(byteLength, itemLength, out var remainder);
        if (remainder is not 0)
            ThrowHelper.ThrowNotEnoughBytesCollection<E>(byteLength);
        return quotient;
    }
}
