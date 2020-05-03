using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T>
    {
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
                var numberLength = MemoryHelper.EncodeNumberLength((uint)length);
                MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)length, numberLength);
                Encode(ref allocator, item);
            }
            else
            {
                var anchor = Allocator.Anchor(ref allocator, sizeof(int));
                Encode(ref allocator, item);
                Allocator.AppendLengthPrefix(ref allocator, anchor, reduce: true);
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
                var allocator = new Allocator(new Span<byte>(buffer), maxCapacity: length);
                Encode(ref allocator, item);
                return buffer;
            }
            else
            {
                var memory = BufferHelper.Borrow();
                try
                {
                    var allocator = new Allocator(BufferHelper.Intent(memory));
                    Encode(ref allocator, item);
                    return Allocator.Result(ref allocator);
                }
                finally
                {
                    BufferHelper.Return(memory);
                }
            }
        }

        public virtual T Decode(byte[] buffer)
        {
            return Decode(new ReadOnlySpan<byte>(buffer));
        }
    }
}
