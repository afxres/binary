namespace Mikodev.Binary.Internal.SpanLike
{
    internal sealed partial class SpanLikeConverter<T, E>
    {
        private void EncodeWithLengthPrefixInternal(ref Allocator allocator, T item)
        {
            var itemLength = this.itemLength;
            if (itemLength is 0)
                EncodeWithLengthPrefixVariable(ref allocator, item);
            else
                EncodeWithLengthPrefixConstant(ref allocator, item);
        }

        private void EncodeWithLengthPrefixConstant(ref Allocator allocator, T item)
        {
            var result = this.create.Handle(item);
            var number = checked(this.itemLength * result.Length);
            var numberLength = NumberHelper.EncodeLength((uint)number);
            NumberHelper.Encode(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
            if (number is 0)
                return;
            this.invoke.Encode(ref allocator, result);
        }

        private void EncodeWithLengthPrefixVariable(ref Allocator allocator, T item)
        {
            var result = this.create.Handle(item);
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            this.invoke.Encode(ref allocator, result);
            Allocator.FinishAnchor(ref allocator, anchor);
        }
    }
}
