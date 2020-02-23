using System;

namespace Mikodev.Binary.Internal
{
    internal abstract partial class ModuleHelper
    {
        public abstract IntPtr AsHandle(ref Allocator allocator);

        public abstract ref Allocator AsAllocator(IntPtr handle);
    }
}
