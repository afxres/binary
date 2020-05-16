using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class AbstractAdapter<T>
    {
        public abstract void EncodeAuto(ref Allocator allocator, T item);

        public abstract void EncodeWithLengthPrefix(ref Allocator allocator, T item);

        public abstract byte[] Encode(T item);

        public abstract T DecodeAuto(ref ReadOnlySpan<byte> span);
    }
}
