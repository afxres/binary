using System;

namespace Mikodev.Binary.Adapters.Abstractions
{
    internal abstract class AdapterMember<T>
    {
        public abstract void Of(ref Allocator allocator, in ReadOnlySpan<T> span);

        public abstract void To(in ReadOnlySpan<byte> span, out T[] result, out int length);
    }
}
