namespace Mikodev.Binary.Internal.SpanLike
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
            var result = this.builder.Handle(item);
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            this.adapter.Encode(ref allocator, result);
            Allocator.FinishAnchor(ref allocator, anchor);
        }
    }
}
