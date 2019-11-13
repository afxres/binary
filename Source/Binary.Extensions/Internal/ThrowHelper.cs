using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    [DebuggerStepThrough]
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTupleNull(Type type) => throw new ArgumentNullException("item", $"Tuple can not be null, type: {type}");
    }
}
