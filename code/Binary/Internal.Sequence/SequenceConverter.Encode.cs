namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed partial class SequenceConverter<T>
    {
        private void EncodeInternal(ref Allocator allocator, T item)
        {
            if (item is null)
                return;
            this.encoder.Invoke(ref allocator, item);
        }
    }
}
