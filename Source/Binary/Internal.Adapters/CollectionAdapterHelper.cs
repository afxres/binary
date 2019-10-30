using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal static class CollectionAdapterHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetItemCount(int byteCount, int definition)
        {
            Debug.Assert(byteCount > 0 && definition > 0);
            var quotient = Math.DivRem(byteCount, definition, out var remainder);
            if (remainder != 0)
                ThrowHelper.ThrowNotEnoughBytes();
            return quotient;
        }
    }
}
