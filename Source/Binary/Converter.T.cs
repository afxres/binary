using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T> : Converter
    {
        protected Converter(int length) : base(typeof(T), length) { }

        public abstract void ToBytes(ref Allocator allocator, T item);

        public abstract T ToValue(in ReadOnlySpan<byte> span);

        public abstract void ToBytesWithMark(ref Allocator allocator, T item);

        public abstract T ToValueWithMark(ref ReadOnlySpan<byte> span);

        public virtual void ToBytesWithLengthPrefix(ref Allocator allocator, T item)
        {
            var anchor = allocator.AnchorLengthPrefix();
            ToBytes(ref allocator, item);
            allocator.FinishLengthPrefix(anchor);
        }

        public virtual T ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount < sizeof(int))
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            var length = Endian<int>.Get(ref MemoryMarshal.GetReference(span));
            var buffer = span.Slice(sizeof(int), length);
            var result = ToValue(in buffer);
            span = span.Slice(sizeof(int) + length);
            return result;
        }

        public virtual byte[] ToBytes(T item)
        {
            var length = Length;
            if (length > 0)
            {
                var buffer = new byte[length];
                var allocator = new Allocator(buffer, maxCapacity: length);
                ToBytes(ref allocator, item);
                return buffer;
            }
            else
            {
#if DEBUG
                var allocator = new Allocator();
#else
                var allocator = new Allocator(Internal.Buffer.GetBuffer());
#endif
                ToBytes(ref allocator, item);
                return allocator.ToArray();
            }
        }

        public virtual T ToValue(byte[] buffer) => ToValue(new ReadOnlySpan<byte>(buffer));
    }
}
