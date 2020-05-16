using System;

namespace Mikodev.Binary.Creators.Generics
{
    internal abstract class GenericsAdapter<T, R>
    {
        public abstract void Encode(ref Allocator allocator, T item);

        public abstract R Decode(ReadOnlySpan<byte> span);
    }
}
