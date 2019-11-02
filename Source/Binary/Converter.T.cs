using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T> : Converter
    {
        protected Converter() : this(0) { }

        protected Converter(int length) : base(typeof(T), length) { }

        public abstract void Encode(ref Allocator allocator, T item);

        public abstract T Decode(in ReadOnlySpan<byte> span);

        public virtual void EncodeAuto(ref Allocator allocator, T item)
        {
            var length = Length;
            if (length > 0)
            {
                Encode(ref allocator, item);
            }
            else
            {
                EncodeWithLengthPrefix(ref allocator, item);
            }
        }

        public virtual T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var length = Length;
            if (length > 0)
            {
                var item = Decode(in span);
                span = span.Slice(length);
                return item;
            }
            else
            {
                return DecodeWithLengthPrefix(ref span);
            }
        }

        public virtual void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var length = Length;
            if (length > 0)
            {
                var prefix = length;
                PrimitiveHelper.EncodeNumber(ref allocator, prefix);
                Encode(ref allocator, item);
            }
            else
            {
                var anchor = Allocator.AnchorLengthPrefix(ref allocator);
                Encode(ref allocator, item);
                Allocator.AppendLengthPrefix(ref allocator, anchor);
            }
        }

        public virtual T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return Decode(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }

        public virtual byte[] Encode(T item)
        {
            var length = Length;
            if (length > 0)
            {
                var buffer = new byte[length];
                var allocator = new Allocator(buffer, maxCapacity: length);
                Encode(ref allocator, item);
                return buffer;
            }
            else
            {
                var buffer = BufferHelper.GetBuffer();
                var allocator = new Allocator(buffer);
                Encode(ref allocator, item);
                return allocator.AsSpan().ToArray();
            }
        }

        public virtual T Decode(byte[] buffer)
        {
            return Decode(new ReadOnlySpan<byte>(buffer));
        }
    }
}
