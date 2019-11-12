using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal static class CollectionAdapterHelper
    {
        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetItemCount(int byteLength, int itemLength, Type itemType)
        {
            Debug.Assert(byteLength > 0 && itemLength > 0);
            var quotient = Math.DivRem(byteLength, itemLength, out var remainder);
            if (remainder != 0)
                ThrowHelper.ThrowCollectionBytesInvalid(itemType, byteLength, remainder);
            return quotient;
        }
    }
}
