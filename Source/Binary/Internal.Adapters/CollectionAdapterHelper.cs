using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal static class CollectionAdapterHelper
    {
        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetItemCount(int byteCount, int definition, Type type)
        {
            Debug.Assert(byteCount > 0 && definition > 0);
            var quotient = Math.DivRem(byteCount, definition, out var remainder);
            if (remainder != 0)
                ThrowHelper.ThrowCollectionBytesInvalid(type, byteCount, remainder);
            return quotient;
        }
    }
}
