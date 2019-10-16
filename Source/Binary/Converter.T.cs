using Mikodev.Binary.Internal;
using System;

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
            allocator.LengthPrefixAnchor(out var anchor);
            ToBytes(ref allocator, item);
            allocator.LengthPrefixFinish(anchor);
        }

        public virtual T ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span) => ToValue(PrimitiveHelper.DecodeWithLengthPrefix(ref span));

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
                var buffer = BufferHelper.GetBuffer();
                var allocator = new Allocator(buffer);
                ToBytes(ref allocator, item);
                return allocator.ToArray();
            }
        }

        public virtual T ToValue(byte[] buffer) => ToValue(new ReadOnlySpan<byte>(buffer));
    }
}
