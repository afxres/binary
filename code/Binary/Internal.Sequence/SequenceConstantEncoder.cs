namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed class SequenceConstantEncoder<T> : SequenceAbstractEncoder<T>
    {
        private readonly int itemLength;

        private readonly SequenceCounter<T> counter;

        private readonly SequenceEncoder<T> encoder;

        public SequenceConstantEncoder(SequenceEncoder<T> encoder, SequenceCounter<T> counter, int itemLength)
        {
            this.counter = counter;
            this.encoder = encoder;
            this.itemLength = itemLength;
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var number = item is null ? 0 : checked(this.itemLength * this.counter.Invoke(item));
            var numberLength = MemoryHelper.EncodeNumberLength((uint)number);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
            this.encoder.Encode(ref allocator, item);
        }
    }
}
