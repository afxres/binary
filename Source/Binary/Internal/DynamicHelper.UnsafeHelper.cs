using System;

namespace Mikodev.Binary.Internal
{
    internal static partial class DynamicHelper
    {
        internal abstract class UnsafeHelper
        {
            public abstract IntPtr AsPointer(ref Allocator allocator);

            public abstract ref Allocator AsAllocator(IntPtr pointer);
        }
    }
}
