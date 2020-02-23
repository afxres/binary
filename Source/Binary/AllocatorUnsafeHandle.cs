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
        public AllocatorUnsafeHandle(ref Allocator allocator) => handle = ModuleHelper.Instance.AsHandle(ref allocator);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Allocator AsAllocator() => ref ModuleHelper.Instance.AsAllocator(handle);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => handle == default ? "<Invalid Allocator Handle>" : AsAllocator().ToString();
    }
}
