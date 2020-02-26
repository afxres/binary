using System;

namespace Mikodev.Binary.Internal
{
    internal abstract partial class ModuleHelper
    {
        public static readonly ModuleHelper Instance;

        public abstract IntPtr AsHandle(ref Allocator allocator);

        public abstract ref Allocator AsAllocator(IntPtr handle);
    }
}
