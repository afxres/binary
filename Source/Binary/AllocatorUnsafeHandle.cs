using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public readonly partial struct AllocatorUnsafeHandle
    {
        private readonly IntPtr handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllocatorUnsafeHandle(ref Allocator allocator) => handle = ModuleHelperInstance.AsHandle(ref allocator);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Allocator AsAllocator() => ref ModuleHelperInstance.AsAllocator(handle);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => handle == default ? "<Invalid Allocator Handle>" : AsAllocator().ToString();
    }
}
