using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Sequence
{
    internal static class SequenceMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetCapacity<T>(int byteLength, int itemLength, int fallbackCapacity)
        {
            Debug.Assert(byteLength > 0);
            Debug.Assert(fallbackCapacity > 0);
            return itemLength > 0 ? GetCapacity<T>(byteLength, itemLength) : fallbackCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetCapacity<T>(int byteLength, int itemLength)
        {
            Debug.Assert(byteLength > 0);
            Debug.Assert(itemLength > 0);
            var quotient = Math.DivRem(byteLength, itemLength, out var remainder);
            if (remainder is not 0)
                ThrowHelper.ThrowNotEnoughBytesCollection<T>(byteLength);
            return quotient;
        }

        internal static T[] GetContents<T>(ICollection<T> collection)
        {
            Debug.Assert(collection is not null);
            var length = collection.Count;
            if (length is 0)
                return Array.Empty<T>();
            var buffer = new T[length];
            collection.CopyTo(buffer, 0);
            return buffer;
        }
    }
}
