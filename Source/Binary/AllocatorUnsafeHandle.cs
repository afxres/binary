using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;

namespace Mikodev.Binary
{
    public readonly struct AllocatorUnsafeHandle
    {
        private readonly IntPtr pointer;

        public AllocatorUnsafeHandle(ref Allocator allocator) => pointer = DynamicHelper.UnsafeHelperInstance.AsPointer(ref allocator);

        public ref Allocator AsAllocator() => ref DynamicHelper.UnsafeHelperInstance.AsAllocator(pointer);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly string ToString() => pointer == default ? "<Invalid Allocator Handle>" : AsAllocator().ToString();
    }
}
