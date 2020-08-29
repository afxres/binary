namespace Mikodev.Binary.Internal.Sequence
{
    internal abstract class SequenceAbstractEncoder<T>
    {
        public abstract void EncodeWithLengthPrefix(ref Allocator allocator, T item);
    }
}
