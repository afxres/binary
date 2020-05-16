using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ConstantAdapter<T> : AbstractAdapter<T>
    {
        private readonly Converter<T> converter;

        public ConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            converter.Encode(ref allocator, item);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var converter = this.converter;
            var length = converter.Length;
            var numberLength = MemoryHelper.EncodeNumberLength((uint)length);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)length, numberLength);
            converter.Encode(ref allocator, item);
        }

        public override byte[] Encode(T item)
        {
            var converter = this.converter;
            var length = converter.Length;
            var buffer = new byte[length];
            var allocator = new Allocator(new Span<byte>(buffer), maxCapacity: length);
            converter.Encode(ref allocator, item);
            return buffer;
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var converter = this.converter;
            var length = converter.Length;
            var item = converter.Decode(in span);
            span = span.Slice(length);
            return item;
        }
    }
}
