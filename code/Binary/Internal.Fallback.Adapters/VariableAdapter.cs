namespace Mikodev.Binary.Internal.Fallback.Adapters
{
    internal sealed class VariableAdapter<T> : FallbackAdapter<T>
    {
        private readonly Converter<T> converter;

        public VariableAdapter(Converter<T> converter) => this.converter = converter;

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

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            this.converter.Encode(ref allocator, item);
            Allocator.AppendLengthPrefix(ref allocator, anchor);
        }
    }
}
