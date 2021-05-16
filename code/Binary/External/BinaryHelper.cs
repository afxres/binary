using Mikodev.Binary.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.External
{
    internal static class BinaryHelper
    {
        private static readonly IReadOnlyList<int> Primes = new[]
        {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Join(uint head, uint last) => head * 33 + last;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Load<T>(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

        internal static int GetCapacity(int capacity)
        {
            var result = Primes.FirstOrDefault(x => x >= capacity);
            if (result is 0)
                ThrowHelper.ThrowMaxCapacityOverflow();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetHashCode(ref byte source, int length)
        {
            var result = (uint)length;
            for (; length >= 4; length -= 4, source = ref Unsafe.Add(ref source, 4))
                result = Join(result, Load<uint>(ref source));
            for (; length >= 1; length -= 1, source = ref Unsafe.Add(ref source, 1))
                result = Join(result, Load<byte>(ref source));
            return (int)result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool GetEquality(ref byte source, int length, byte[] buffer)
        {
            if (length != buffer.Length)
                return false;
            if (length is 0)
                return true;
            ref var origin = ref MemoryMarshal.GetArrayDataReference(buffer);
            for (; length >= 4; length -= 4, source = ref Unsafe.Add(ref source, 4), origin = ref Unsafe.Add(ref origin, 4))
                if (Load<uint>(ref source) != Load<uint>(ref origin))
                    return false;
            for (; length >= 1; length -= 1, source = ref Unsafe.Add(ref source, 1), origin = ref Unsafe.Add(ref origin, 1))
                if (Load<byte>(ref source) != Load<byte>(ref origin))
                    return false;
            return true;
        }
    }
}
