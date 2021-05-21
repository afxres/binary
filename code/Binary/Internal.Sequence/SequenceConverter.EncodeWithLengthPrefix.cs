namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed partial class SequenceConverter<T>
    {
        private void EncodeWithLengthPrefixInternal(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            this.encode.Invoke(ref allocator, item);
            Allocator.FinishAnchor(ref allocator, anchor);
        }
    }
}
