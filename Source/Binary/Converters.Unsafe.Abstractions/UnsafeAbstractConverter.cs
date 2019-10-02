using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Converters.Unsafe.Abstractions
{
    internal abstract class UnsafeAbstractConverter<T, L> : ConstantConverter<T> where L : unmanaged
    {
        protected UnsafeAbstractConverter() : base(Memory.SizeOf<L>()) { }

        protected abstract void Of(ref byte location, T item);

        protected abstract T To(ref byte location);

        public sealed override void ToBytes(ref Allocator allocator, T item)
        {
            Of(ref allocator.AllocateReference(Memory.SizeOf<L>()), item);
        }

        public sealed override T ToValue(in ReadOnlySpan<byte> span)
        {
            if (span.Length < Memory.SizeOf<L>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            return To(ref MemoryMarshal.GetReference(span));
        }

        public sealed override byte[] ToBytes(T item)
        {
            var buffer = new byte[Memory.SizeOf<L>()];
            Of(ref buffer[0], item);
            return buffer;
        }

        public sealed override T ToValue(byte[] buffer)
        {
            if (buffer == null || buffer.Length < Memory.SizeOf<L>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            return To(ref buffer[0]);
        }

        public sealed override void ToBytesWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var memory = ref allocator.AllocateReference(sizeof(int) + Memory.SizeOf<L>());
            Endian<int>.Set(ref memory, Memory.SizeOf<L>());
            Of(ref Memory.Add(ref memory, sizeof(int)), item);
        }
    }
}
