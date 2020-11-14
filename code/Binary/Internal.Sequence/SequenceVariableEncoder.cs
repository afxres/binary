namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed class SequenceVariableEncoder<T> : SequenceAbstractEncoder<T>
    {
        private readonly SequenceEncoder<T> encoder;

        public SequenceVariableEncoder(SequenceEncoder<T> encoder) => this.encoder = encoder;

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            this.encoder.Encode(ref allocator, item);
            Allocator.FinishAnchor(ref allocator, anchor);
        }
    }
}
