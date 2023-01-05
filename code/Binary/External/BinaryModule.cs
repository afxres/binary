﻿namespace Mikodev.Binary.External;

using Mikodev.Binary.External.Contexts;
using Mikodev.Binary.Internal;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BIN1 = System.Byte;
using BIN2 = System.UInt16;
using BIN4 = System.UInt32;
using BIN8 = System.UInt64;

internal static class BinaryModule
{
    private static readonly ImmutableArray<int> Primes = ImmutableArray.Create(new[]
    {
        3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
        1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
        17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
        187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
        1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369,
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Join(uint head, uint last) => head * 33 + last;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Load<T>(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Load<T>(ref byte source, int offset) => Unsafe.ReadUnaligned<T>(ref Unsafe.Add(ref source, offset));

    internal static int GetCapacity(int capacity)
    {
        var result = Primes.FirstOrDefault(x => x >= capacity);
        if (result is 0)
            ThrowHelper.ThrowMaxCapacityOverflow();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint GetHashCode(ref byte source, int length)
    {
        var result = (uint)length;
        for (; length >= 4; length -= 4, source = ref Unsafe.Add(ref source, 4))
            result = Join(result, Load<uint>(ref source));
        for (; length >= 1; length -= 1, source = ref Unsafe.Add(ref source, 1))
            result = Join(result, Load<byte>(ref source));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool GetEquality(ref byte source, int length, byte[] buffer)
    {
        if (length != buffer.Length)
            return false;
        if (length is 0)
            return true;
        ref var origin = ref MemoryMarshal.GetArrayDataReference(buffer);
        for (; length >= 8; length -= 8, source = ref Unsafe.Add(ref source, 8), origin = ref Unsafe.Add(ref origin, 8))
            if (Load<BIN8>(ref source) != Load<BIN8>(ref origin))
                return false;
        if ((length & 4) is not 0 && Load<BIN4>(ref source, length & 3) != Load<BIN4>(ref origin, length & 3))
            return false;
        if ((length & 2) is not 0 && Load<BIN2>(ref source, length & 1) != Load<BIN2>(ref origin, length & 1))
            return false;
        if ((length & 1) is not 0 && Load<BIN1>(ref source, length & 0) != Load<BIN1>(ref origin, length & 0))
            return false;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static LongDataSlot GetLongData(ref byte source, int length)
    {
        Debug.Assert(length >= 0);
        Debug.Assert(length <= 15);
        var head = (length & 8) is 0 ? 0UL : Load<BIN8>(ref source, length & 7);
        var tail = (length & 4) is 0 ? 0UL : Load<BIN4>(ref source, length & 3);
        if ((length & 2) is not 0)
            tail = (tail << 0x10) | Load<BIN2>(ref source, length & 1);
        if ((length & 1) is not 0)
            tail = (tail << 0x08) | Load<BIN1>(ref source, length & 0);
        return new LongDataSlot { Head = head, Tail = (tail << 0x08) | (uint)length };
    }
}
