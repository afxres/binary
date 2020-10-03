namespace Mikodev.Binary.Internal.SpanLike
{
    internal abstract class SpanLikeAbstractEncoder<T>
    {
        public abstract void EncodeWithLengthPrefix(ref Allocator allocator, T item);
    }
}
