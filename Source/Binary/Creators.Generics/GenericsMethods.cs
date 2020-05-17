using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Creators.Generics
{
    internal static class GenericsMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetCapacity(int byteLength, int itemLength, Type itemType)
        {
            Debug.Assert(byteLength > 0);
            Debug.Assert(itemLength > 0);
            var quotient = Math.DivRem(byteLength, itemLength, out var remainder);
            if (remainder != 0)
                ThrowHelper.ThrowCollectionBytesInvalid(itemType, byteLength, remainder);
            return quotient;
        }

        internal static T[] GetContents<T>(ICollection<T> collection)
        {
            Debug.Assert(collection != null);
            var length = collection.Count;
            if (length == 0)
                return Array.Empty<T>();
            var buffer = new T[length];
            collection.CopyTo(buffer, 0);
            return buffer;
        }
    }
}
