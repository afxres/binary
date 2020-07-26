namespace Mikodev.Binary.Creators.Sequence
{
    internal sealed class SequenceVariableEncoder<T, R> : SequenceAbstractEncoder<T>
    {
        private readonly SequenceAdapter<T, R> adapter;

        public SequenceVariableEncoder(SequenceAdapter<T, R> adapter)
        {
            this.adapter = adapter;
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            adapter.Encode(ref allocator, item);
            Allocator.AppendLengthPrefix(ref allocator, anchor);
        }
    }
}
