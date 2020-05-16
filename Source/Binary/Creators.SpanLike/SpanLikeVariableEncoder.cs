namespace Mikodev.Binary.Creators.SpanLike
{
    internal sealed class SpanLikeVariableEncoder<T, E> : SpanLikeAbstractEncoder<T>
    {
        private readonly SpanLikeAdapter<E> adapter;

        private readonly SpanLikeBuilder<T, E> builder;

        public SpanLikeVariableEncoder(SpanLikeAdapter<E> adapter, SpanLikeBuilder<T, E> builder)
        {
            this.adapter = adapter;
            this.builder = builder;
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var data = builder.Handle(item);
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            adapter.Encode(ref allocator, data);
            Allocator.AppendLengthPrefix(ref allocator, anchor, reduce: true);
        }
    }
}
