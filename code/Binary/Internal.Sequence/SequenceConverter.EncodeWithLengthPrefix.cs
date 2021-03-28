namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed partial class SequenceConverter<T>
    {
        private void EncodeWithLengthPrefixInternal(ref Allocator allocator, T item)
        {
            if (item is null)
            {
                var number = 0;
                var numberLength = 1;
                MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
            }
            else
            {
                var anchor = Allocator.Anchor(ref allocator, sizeof(int));
                this.encoder.Invoke(ref allocator, item);
                Allocator.FinishAnchor(ref allocator, anchor);
            }
        }
    }
}
