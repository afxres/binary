using System;

namespace Mikodev.Binary.Internal.Fallback.Adapters
{
    internal sealed class FallbackVariableAdapter<T> : FallbackAdapter<T>
    {
        private readonly Converter<T> converter;

        public FallbackVariableAdapter(Converter<T> converter) => this.converter = converter;

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            this.converter.EncodeWithLengthPrefix(ref allocator, item);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            this.converter.Encode(ref allocator, item);
            Allocator.AppendLengthPrefix(ref allocator, anchor);
        }

        public override byte[] Encode(T item)
        {
            var handle = BufferHelper.Borrow();
            try
            {
                var allocator = new Allocator(BufferHelper.Result(handle));
                this.converter.Encode(ref allocator, item);
                return allocator.ToArray();
            }
            finally
            {
                BufferHelper.Return(handle);
            }
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            return this.converter.DecodeWithLengthPrefix(ref span);
        }
    }
}
