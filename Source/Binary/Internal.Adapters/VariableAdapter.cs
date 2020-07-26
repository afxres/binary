using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class VariableAdapter<T> : AbstractAdapter<T>
    {
        private readonly Converter<T> converter;

        public VariableAdapter(Converter<T> converter) => this.converter = converter;

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            converter.EncodeWithLengthPrefix(ref allocator, item);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            converter.Encode(ref allocator, item);
            Allocator.AppendLengthPrefix(ref allocator, anchor);
        }

        public override byte[] Encode(T item)
        {
            var memory = BufferHelper.Borrow();
            try
            {
                var allocator = new Allocator(BufferHelper.Intent(memory));
                converter.Encode(ref allocator, item);
                return Allocator.Result(ref allocator);
            }
            finally
            {
                BufferHelper.Return(memory);
            }
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            return converter.DecodeWithLengthPrefix(ref span);
        }
    }
}
