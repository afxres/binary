using Mikodev.Binary.Internal;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T>
    {
        private void EncodeWithLengthPrefixInternal(ref Allocator allocator, T item)
        {
            var length = this.length;
            if (length is not 0)
                EncodeWithLengthPrefixConstant(ref allocator, item);
            else
                EncodeWithLengthPrefixVariable(ref allocator, item);
        }

        private void EncodeWithLengthPrefixConstant(ref Allocator allocator, T item)
        {
            var length = this.length;
            var numberLength = MemoryHelper.EncodeNumberLength((uint)length);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)length, numberLength);
            Encode(ref allocator, item);
        }

        private void EncodeWithLengthPrefixVariable(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            Encode(ref allocator, item);
            Allocator.FinishAnchor(ref allocator, anchor);
        }
    }
}
