using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.External
{
    internal static class BinaryHelper
    {
        private static readonly IReadOnlyList<int> primes = new[]
        {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Join(int head, int last) => (head << 5) + head + last;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Load<T>(ref byte source, int offset) => Unsafe.ReadUnaligned<T>(ref Unsafe.Add(ref source, offset));

        internal static int GetCapacity(int capacity)
        {
            return primes.First(x => x > capacity);
        }

        internal static int GetHashCode(ref byte source, int length)
        {
            var result = length;
            var header = length >> 2;
            for (var i = 0; i < header; i++)
                result = Join(result, Load<int>(ref source, i << 2));
            for (var i = header << 2; i < length; i++)
                result = Join(result, Load<byte>(ref source, i));
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool GetEquality(ref byte source, int length, byte[] buffer)
        {
            Debug.Assert(buffer is not null);
#if NET5_0_OR_GREATER
            var cursor = buffer.Length;
            ref var origin = ref MemoryMarshal.GetArrayDataReference(buffer);
            return MemoryExtensions.SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref source, length), MemoryMarshal.CreateReadOnlySpan(ref origin, cursor));
#else
            return MemoryExtensions.SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref source, length), new ReadOnlySpan<byte>(buffer));
#endif
        }
    }
}
