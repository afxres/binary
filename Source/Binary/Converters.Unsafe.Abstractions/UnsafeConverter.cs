using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Converters.Unsafe.Abstractions
{
    internal abstract class UnsafeConverter<TData, TSize> : ConstantConverter<TData> where TSize : unmanaged
    {
        protected UnsafeConverter() : base(Memory.SizeOf<TSize>()) { }

        #region abstract methods
        public abstract void OfValue(ref byte location, TData item);

        public abstract TData ToValue(ref byte location);
        #endregion

        #region basic methods
        public sealed override void ToBytes(ref Allocator allocator, TData item)
        {
            OfValue(ref allocator.AllocateReference(Memory.SizeOf<TSize>()), item);
        }

        public sealed override TData ToValue(in ReadOnlySpan<byte> span)
        {
            if (span.Length < Memory.SizeOf<TSize>())
                return ThrowHelper.ThrowNotEnoughBytes<TData>();
            return ToValue(ref MemoryMarshal.GetReference(span));
        }
        #endregion

        #region basic methods (via bytes)
        public sealed override byte[] ToBytes(TData item)
        {
            var buffer = new byte[Memory.SizeOf<TSize>()];
            OfValue(ref buffer[0], item);
            return buffer;
        }

        public sealed override TData ToValue(byte[] buffer)
        {
            if (buffer == null || buffer.Length < Memory.SizeOf<TSize>())
                return ThrowHelper.ThrowNotEnoughBytes<TData>();
            return ToValue(ref buffer[0]);
        }
        #endregion

        #region other methods
        public sealed override void ToBytesWithLengthPrefix(ref Allocator allocator, TData item)
        {
            ref var memory = ref allocator.AllocateReference(sizeof(int) + Memory.SizeOf<TSize>());
            Endian<int>.Set(ref memory, Memory.SizeOf<TSize>());
            OfValue(ref Memory.Add(ref memory, sizeof(int)), item);
        }
        #endregion
    }
}
