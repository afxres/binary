using Mikodev.Binary.Internal;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.Sequence
{
    internal sealed class SequenceConstantEncoder<T, R> : SequenceAbstractEncoder<T>
    {
        private readonly int itemLength;

        private readonly SequenceAdapter<T, R> adapter;

        private readonly SequenceCounter<T> counter;

        public SequenceConstantEncoder(SequenceAdapter<T, R> adapter, SequenceCounter<T> counter, int itemLength)
        {
            this.adapter = adapter;
            this.counter = counter;
            this.itemLength = itemLength;
            Debug.Assert(itemLength > 0);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var size = item is null ? 0 : counter.Invoke(item);
            var number = checked(itemLength * size);
            var numberLength = MemoryHelper.EncodeNumberLength((uint)number);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
            adapter.Encode(ref allocator, item);
        }
    }
}
