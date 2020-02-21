using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public readonly struct AllocatorUnsafeHandle
    {
        private readonly IntPtr handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllocatorUnsafeHandle(ref Allocator allocator) => handle = DynamicHelper.UnsafeHelperInstance.AsPointer(ref allocator);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Allocator AsAllocator() => ref DynamicHelper.UnsafeHelperInstance.AsAllocator(handle);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly string ToString() => handle == default ? "<Invalid Allocator Handle>" : AsAllocator().ToString();
    }
}
