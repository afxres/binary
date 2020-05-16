namespace Mikodev.Binary.Creators.SpanLike
{
    internal abstract class SpanLikeAbstractEncoder<T>
    {
        public abstract void EncodeWithLengthPrefix(ref Allocator allocator, T item);
    }
}
