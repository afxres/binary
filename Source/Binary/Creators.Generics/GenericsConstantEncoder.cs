using Mikodev.Binary.Internal;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.Generics
{
    internal sealed class GenericsConstantEncoder<T, R> : GenericsAbstractEncoder<T>
    {
        private readonly int itemLength;

        private readonly GenericsAdapter<T, R> adapter;

        private readonly GenericsCounter<T> counter;

        public GenericsConstantEncoder(GenericsAdapter<T, R> adapter, GenericsCounter<T> counter, int itemLength)
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
